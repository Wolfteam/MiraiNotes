using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Android.Models.Results;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class TaskMenuOptionsViewModel : BaseViewModel<TaskMenuOptionsViewModelParameter>
    {
        #region Members
        private TaskListItemViewModel _taskList;
        private TaskItemViewModel _task;
        private string _markAsTitle;
        private bool _showAddSubTaskButton;
        private MvxInteraction<TaskItemViewModel> _shareTask = new MvxInteraction<TaskItemViewModel>();
        #endregion

        #region Interactors
        public IMvxInteraction<TaskItemViewModel> ShareTask
            => _shareTask;

        #endregion
        #region Properties
        public string MarkAsTitle
        {
            get => _markAsTitle;
            set => SetProperty(ref _markAsTitle, value);
        }

        public bool ShowAddSubTaskButton
        {
            get => _showAddSubTaskButton;
            set => SetProperty(ref _showAddSubTaskButton, value);
        }
        #endregion

        #region Commands
        public IMvxAsyncCommand DeleteTaskCommand { get; private set; }
        public IMvxAsyncCommand ChangeTaskStatusCommand { get; private set; }
        public IMvxAsyncCommand AddSubTaskCommand { get; private set; }
        public IMvxAsyncCommand MoveTaskCommand { get; private set; }
        public IMvxAsyncCommand AddReminderCommand { get; private set; }
        public IMvxAsyncCommand ShareCommand { get; private set; } 
        #endregion

        public TaskMenuOptionsViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<TaskMenuOptionsViewModel>(), navigationService, appSettings, telemetryService)
        {
        }

        public override void Prepare(TaskMenuOptionsViewModelParameter parameter)
        {
            base.Prepare(parameter);
            _taskList = parameter.TaskList;
            _task = parameter.Task;
            string statusMessage =
                $"{(_task.IsCompleted ? GetText("Incompleted") : GetText("Completed"))}";
            MarkAsTitle = GetText("MarkTaskAs", statusMessage);
            ShowAddSubTaskButton = !_task.HasParentTask;
        }

        public override void SetCommands()
        {
            base.SetCommands();
            DeleteTaskCommand = new MvxAsyncCommand(async () =>
            {
                var parameter = DeleteTaskDialogViewModelParameter.Delete(_task);
                await NavigationService.Close(this);
                await NavigationService.Navigate<DeleteTaskDialogViewModel, DeleteTaskDialogViewModelParameter, bool>(parameter);
            });

            ChangeTaskStatusCommand = new MvxAsyncCommand(async () =>
            {
                await NavigationService.Close(this);
                await NavigationService.Navigate<ChangeTaskStatusDialogViewModel, TaskItemViewModel, bool>(_task);
            });

            AddSubTaskCommand = new MvxAsyncCommand(async () =>
            {
                var parameter = AddSubTaskDialogViewModelParameter.Instance(_taskList.GoogleId, _task);
                await NavigationService.Close(this);
                await NavigationService.Navigate<AddSubTaskDialogViewModel, AddSubTaskDialogViewModelParameter, bool>(parameter);
            });

            MoveTaskCommand = new MvxAsyncCommand(async () =>
            {
                var parameter = TaskListsDialogViewModelParameter.MoveTo(_taskList, _task);
                await NavigationService.Close(this);
                await NavigationService
                    .Navigate<TaskListsDialogViewModel, TaskListsDialogViewModelParameter, TaskListsDialogViewModelResult>(parameter);
            });

            AddReminderCommand = new MvxAsyncCommand(async () =>
            {
                var parameter = TaskDateViewModelParameter.Instance(_taskList, _task, Core.Enums.TaskNotificationDateType.REMINDER_DATE);
                await NavigationService.Close(this);
                await NavigationService.Navigate<TaskDateDialogViewModel, TaskDateViewModelParameter, bool>(parameter);
            });

            ShareCommand = new MvxAsyncCommand(async() =>
            {
                _shareTask.Raise(Parameter.Task);
                await NavigationService.Close(this);
            });
        }
    }
}