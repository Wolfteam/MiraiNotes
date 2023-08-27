using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Android.Models.Results;
using MiraiNotes.Android.ViewModels.Dialogs;
using MiraiNotes.Android.ViewModels.Settings;
using MiraiNotes.Core.Enums;
using MiraiNotes.Shared.Utils;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;
using System;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Members
        private readonly IUserCredentialService _userCredentialService;
        private readonly IDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IBackgroundTaskManagerService _backgroundTaskManager;
        private readonly MvxInteraction<bool> _showDrawer = new MvxInteraction<bool>();
        private readonly MvxInteraction<AppThemeChangedMsg> _appThemeChanged = new MvxInteraction<AppThemeChangedMsg>();
        private readonly MvxInteraction _appLanguageChanged = new MvxInteraction();
        private readonly MvxInteraction _hideKeyboard = new MvxInteraction();
        private readonly MvxInteraction<TaskSortType> _updateTaskSortOrder = new MvxInteraction<TaskSortType>();
        private readonly MvxInteraction<bool> _changeTasksSelectionMode = new MvxInteraction<bool>();

        private bool _showProgressOverlay;
        private string _overlayText;

        public bool IsInSelectionMode;
        #endregion

        #region Interactors
        public IMvxInteraction<bool> ShowDrawer
            => _showDrawer;

        public IMvxInteraction<AppThemeChangedMsg> AppThemeChanged
            => _appThemeChanged;

        public IMvxInteraction AppLanguageChanged
            => _appLanguageChanged;

        public IMvxInteraction HideKeyboard
            => _hideKeyboard;

        public IMvxInteraction<TaskSortType> UpdateTaskSortOrder
            => _updateTaskSortOrder;

        public IMvxInteraction<bool> ChangeTasksSelectionMode
            => _changeTasksSelectionMode;
        #endregion

        #region Properties
        public bool ShowProgressOverlay
        {
            get => _showProgressOverlay;
            set => SetProperty(ref _showProgressOverlay, value);
        }

        public string OverlayText
        {
            get => _overlayText;
            set => SetProperty(ref _overlayText, value);
        }

        public NotificationAction InitParams { get; set; }
        #endregion

        #region Commands
        public IMvxAsyncCommand OnSettingsSelectedCommand { get; private set; }
        public IMvxAsyncCommand OnAccountsSelectedCommand { get; private set; }
        public IMvxAsyncCommand LogoutCommand { get; private set; }
        public IMvxAsyncCommand<bool> InitViewCommand { get; private set; }
        public IMvxCommand SyncCommand { get; private set; }
        public IMvxCommand<TaskSortType> TaskSortOrderChangedCommand { get; private set; }
        public IMvxAsyncCommand ManageTaskListsCommand { get; private set; }
        #endregion

        public MainViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IDialogService dialogService,
            IUserCredentialService userCredentialService,
            IMiraiNotesDataService dataService,
            IAppSettingsService appSettings,
            IBackgroundTaskManagerService backgroundTaskManager,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<MainViewModel>(), navigationService, appSettings, telemetryService)
        {
            _userCredentialService = userCredentialService;
            _dialogService = dialogService;
            _dataService = dataService;
            _backgroundTaskManager = backgroundTaskManager;
        }

        public override Task Initialize()
        {
            try
            {
                Logger.Information($"{nameof(Prepare)}: Trying to delete old logs...");
                var logsPath = AndroidUtils.GetLogsPath();
                FileUtils.DeleteFilesInDirectory(logsPath, DateTime.Now.AddDays(-3));
            }
            catch (Exception e)
            {
                Logger.Error(e, $"{nameof(Prepare)}: Unknonw error while trying to delete old logs");
                TelemetryService.TrackError(e);
            }
            return base.Initialize();
        }

        public override void SetCommands()
        {
            base.SetCommands();
            OnSettingsSelectedCommand = new MvxAsyncCommand(
                async () => await NavigationService.Navigate<SettingsMainViewModel>());
            OnAccountsSelectedCommand = new MvxAsyncCommand(OnAccountsSelected);
            LogoutCommand = new MvxAsyncCommand(async () =>
            {
                var logout = await NavigationService.Navigate<LogoutDialogViewModel, bool?, NavigationBoolResult>(null);

                if (logout?.Result ?? false)
                    await Logout();
            });
            InitViewCommand = new MvxAsyncCommand<bool>((orientationChanged) =>
            {
                var parameter = MenuViewModelParameter.Instance(InitParams, orientationChanged);
                return NavigationService.Navigate<MenuViewModel, MenuViewModelParameter>(parameter);
            });

            SyncCommand = new MvxCommand(
                () => _backgroundTaskManager.StartBackgroundTask(BackgroundTaskType.SYNC));

            TaskSortOrderChangedCommand = new MvxCommand<TaskSortType>(
                (sortType) => Messenger.Publish(new TaskSortOrderChangedMsg(this, sortType)));

            ManageTaskListsCommand = new MvxAsyncCommand(
                () => NavigationService.Navigate<ManageTaskListsDialogViewModel>());
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();
            var tokens = new[]
            {
                Messenger.Subscribe<ShowDrawerMsg>(msg => _showDrawer.Raise(msg.Show)),
                Messenger.Subscribe<AppThemeChangedMsg>(msg => _appThemeChanged.Raise(msg)),
                Messenger.Subscribe<AppLanguageChangedMessage>(msg =>
                {
                    if (msg.RestartActivity)
                        _appLanguageChanged.Raise();
                }),
                Messenger.Subscribe<HideKeyboardMsg>(_ => _hideKeyboard.Raise()),
                Messenger.Subscribe<ShowProgressOverlayMsg>(msg =>
                {
                    ShowProgressOverlay = msg.Show;
                    OverlayText = !string.IsNullOrEmpty(msg.Msg) ? msg.Msg : $"{TextProvider.Get("Loading")}...";
                }),
                Messenger.Subscribe<TaskSortOrderChangedMsg>(msg => _updateTaskSortOrder.Raise(msg.NewSortOrder)),
                Messenger.Subscribe<ChangeTasksSelectionModeMsg>(msg => _changeTasksSelectionMode.Raise(msg.IsInSelectionMode))
            };

            SubscriptionTokens.AddRange(tokens);
        }

        public async Task Logout()
        {
            //delete all from the db
            //delete user settings
            //delete all view models
            Messenger.Publish(new ShowProgressOverlayMsg(this));

            _backgroundTaskManager.UnregisterBackgroundTasks(BackgroundTaskType.ANY);
            AppSettings.ResetAppSettings();

            await _dataService
                .UserService
                .ChangeCurrentUserStatus(false);

            string currentLoggedUsername = _userCredentialService.GetCurrentLoggedUsername();
            if (string.IsNullOrEmpty(currentLoggedUsername))
                currentLoggedUsername = _userCredentialService.DefaultUsername;

            _userCredentialService.DeleteUserCredential(ResourceType.ALL, currentLoggedUsername);

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            await NavigationService.Close(this);
            await NavigationService.Navigate<LoginViewModel>();
        }

        private async Task OnAccountsSelected()
        {
            Messenger.Publish(new ShowDrawerMsg(this, false));
            await NavigationService.Navigate<AccountDialogViewModel>();
        }
    }
}