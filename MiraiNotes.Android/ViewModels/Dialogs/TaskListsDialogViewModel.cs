using AutoMapper;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Android.Models.Results;
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
    public class TaskListsDialogViewModel : BaseConfirmationDialogViewModel<TaskListsDialogViewModelParameter, TaskListsDialogViewModelResult>
    {
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;
        private readonly IMapper _mapper;
        private MvxObservableCollection<TaskListItemViewModel> _taskLists = new MvxObservableCollection<TaskListItemViewModel>();
        private TaskListItemViewModel _currentTaskList;
        private readonly MvxInteraction<bool> _hideDialog = new MvxInteraction<bool>();

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

        public IMvxInteraction<bool> HideDialog
            => _hideDialog;

        public IMvxAsyncCommand<TaskListItemViewModel> TaskListSelectedCommand { get; private set; }

        public TaskListsDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService,
            IMiraiNotesDataService dataService,
            IDialogService dialogService,
            IMapper mapper)
            : base(textProvider, messenger, logger.ForContext<TaskListsDialogViewModel>(), navigationService, appSettings, telemetryService)
        {
            _dataService = dataService;
            _dialogService = dialogService;
            _mapper = mapper;
        }

        public override void Prepare(TaskListsDialogViewModelParameter parameter)
        {
            base.Prepare(parameter);
            CurrentTaskList = parameter.TaskList;
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

                if (Parameter.Move)
                    await MoveToTaskList(selectedTaskList);
                else
                    await SetSelectedTaskList(selectedTaskList);
            });
        }

        private async Task MoveToTaskList(TaskListItemViewModel selectedTaskList)
        {
            _hideDialog.Raise(true);

            var parameter = MoveTaskDialogViewModelParameter.Instance(Parameter.TaskList, selectedTaskList, Parameter.Task);
            bool wasMoved = await NavigationService.Navigate<MoveTaskDialogViewModel, MoveTaskDialogViewModelParameter, bool>(parameter);
            if (wasMoved)
            {
                var result = TaskListsDialogViewModelResult.Moved(Parameter.TaskList);
                await NavigationService.Close(this, result);
            }
            else
            {
                CurrentTaskList = TaskLists.First(t => t.Id == Parameter.TaskList.Id);
                _hideDialog.Raise(false);
            }
        }

        private async Task SetSelectedTaskList(TaskListItemViewModel selectedTaskList)
        {
            var result = TaskListsDialogViewModelResult.Selected(selectedTaskList);

            await NavigationService.Close(this, result);
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