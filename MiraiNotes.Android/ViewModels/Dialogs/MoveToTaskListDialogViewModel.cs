using AutoMapper;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Core.Enums;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class MoveToTaskListDialogViewModel : BaseConfirmationDialogViewModel<MoveToTaskListDialogViewModelParameter, bool>
    {
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;
        private readonly IMapper _mapper;
        private MvxObservableCollection<TaskListItemViewModel> _taskLists = new MvxObservableCollection<TaskListItemViewModel>();
        private TaskListItemViewModel _currentTaskList;
        private readonly MvxInteraction _hideDialog = new MvxInteraction();

        public MvxObservableCollection<TaskListItemViewModel> TaskLists
        {
            get => _taskLists;
            set => SetProperty(ref _taskLists, value);
        }

        public TaskListItemViewModel CurrentTaskList
        {
            get => _currentTaskList;
            set
            {
                SetProperty(ref _currentTaskList, value);
                UpdateSelectedItem();
            }
        }

        public IMvxInteraction HideDialog
            => _hideDialog;

        public IMvxAsyncCommand<TaskListItemViewModel> TaskListSelectedCommand { get; private set; }

        public MoveToTaskListDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService,
            IMiraiNotesDataService dataService,
            IDialogService dialogService,
            IMapper mapper)
            : base(textProvider, messenger, logger.ForContext<MoveToTaskListDialogViewModel>(), navigationService, appSettings, telemetryService)
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

        public override void SetCommands()
        {
            base.SetCommands();

            TaskListSelectedCommand = new MvxAsyncCommand<TaskListItemViewModel>(async (selectedTaskList) =>
            {
                if (selectedTaskList.GoogleId == Parameter.TaskList.GoogleId)
                    return;

                _hideDialog.Raise();

                var parameter = MoveTaskDialogViewModelParameter.Instance(Parameter.TaskList, selectedTaskList, Parameter.Task);
                bool wasMoved = await NavigationService.Navigate<MoveTaskDialogViewModel, MoveTaskDialogViewModelParameter, bool>(parameter);
                await NavigationService.Close(this, wasMoved);
            });
        }

        private async Task GetAllTaskLists()
        {
            Messenger.Publish(new ShowProgressOverlayMsg(this));

            var dbResponse = await _dataService
                .TaskListService
                .GetAsNoTrackingAsync(
                    tl => tl.User.IsActive && tl.LocalStatus != LocalStatus.DELETED,
                    tl => tl.OrderBy(t => t.Title));

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

                CurrentTaskList = TaskLists.FirstOrDefault(tl => tl.GoogleId == Parameter.TaskList.GoogleId);
            }

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));
        }

        private void UpdateSelectedItem()
        {
            foreach (var tl in TaskLists)
            {
                tl.IsSelected = tl.GoogleId == _currentTaskList.GoogleId
                    ? true
                    : false;
            }
        }
    }
}