using AutoMapper;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Results;
using MiraiNotes.Core.Enums;
using MiraiNotes.Shared.Extensions;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class ManageTaskListsDialogViewModel : BaseViewModel
    {
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;
        private readonly IMapper _mapper;
        private MvxObservableCollection<TaskListItemViewModel> _taskLists = new MvxObservableCollection<TaskListItemViewModel>();
        private readonly MvxInteraction<bool> _hideDialog = new MvxInteraction<bool>();

        public MvxObservableCollection<TaskListItemViewModel> TaskLists
        {
            get => _taskLists;
            set => SetProperty(ref _taskLists, value);
        }

        public IMvxInteraction<bool> HideDialog
            => _hideDialog;

        public ManageTaskListsDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService,
            IMiraiNotesDataService dataService,
            IDialogService dialogService,
            IMapper mapper)
            : base(textProvider, messenger, logger.ForContext<ManageTaskListsDialogViewModel>(), navigationService, appSettings, telemetryService)
        {
            _dataService = dataService;
            _dialogService = dialogService;
            _mapper = mapper;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await GetAllTaskLists();
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();

            var tokens = new[]
            {
                Messenger.Subscribe<OnManageTaskListItemClickMsg>(async msg => await OnItemClick(msg))
            };

            SubscriptionTokens.AddRange(tokens);
        }

        private async Task GetAllTaskLists()
        {
            Messenger.Publish(new ShowProgressOverlayMsg(this));

            var dbResponse = await _dataService
                .TaskListService
                .GetAsNoTrackingAsync(
                    tl => tl.User.IsActive && tl.LocalStatus != LocalStatus.DELETED);

            if (!dbResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(GetAllTaskLists)}: An unknown error occurred while trying to retrieve all " +
                    $"the task lists from the db. Error = {dbResponse.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
            }
            else
            {
                var taskLists = _mapper.Map<List<TaskListItemViewModel>>(dbResponse.Result);
                TaskLists.AddRange(taskLists);
                TaskLists.SortBy(tl => tl.Title);
            }

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));
        }

        private async Task OnItemClick(OnManageTaskListItemClickMsg msg)
        {
            if (msg.Delete && TaskLists.Count == 1)
            {
                _dialogService.ShowWarningToast("You cant delete the last task list");
                return;
            }

            _hideDialog.Raise(true);
            if (msg.Edit)
            {
                var result = await NavigationService
                    .Navigate<AddEditTaskListDialogViewModel, TaskListItemViewModel, AddEditTaskListDialogViewModelResult>(msg.TaskList);
                if (result == null || result.NoChangesWereMade)
                {
                    _hideDialog.Raise(false);
                }
                else
                {
                    await NavigationService.Close(this);
                }
            }
            else
            {
                bool wasDeleted = await NavigationService
                    .Navigate<DeleteTaskListDialogViewModel, TaskListItemViewModel, bool>(msg.TaskList);

                if (!wasDeleted)
                {
                    _hideDialog.Raise(false);
                }
                else
                {
                    await NavigationService.Close(this);
                }
            }
        }
    }
}