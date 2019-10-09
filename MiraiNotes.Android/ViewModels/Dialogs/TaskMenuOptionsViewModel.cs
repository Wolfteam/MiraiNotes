using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Parameters;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class TaskMenuOptionsViewModel : BaseViewModel<TaskMenuOptionsViewModelParameter>
    {
        private TaskListItemViewModel _taskList;
        private TaskItemViewModel _task;
        private string _markAsTitle;

        public string MarkAsTitle
        {
            get => _markAsTitle;
            set => SetProperty(ref _markAsTitle, value);
        }

        public IMvxAsyncCommand DeleteTaskCommand { get; private set; }
        public IMvxAsyncCommand ChangeTaskStatusCommand { get; private set; }
        public IMvxAsyncCommand AddSubTaskCommand { get; private set; }
        public IMvxAsyncCommand MoveTaskCommand { get; private set; }
        public IMvxAsyncCommand AddReminderCommand { get; private set; }

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
            _taskList = parameter.TaskList;
            _task = parameter.Task;
            string statusMessage =
                $"{(_task.IsCompleted ? GetText("Incompleted") : GetText("Completed"))}";
            MarkAsTitle = GetText("MarkTaskAs", statusMessage);
        }

        public override void SetCommands()
        {
            base.SetCommands();
            DeleteTaskCommand = new MvxAsyncCommand(async () =>
            {
                await NavigationService.Close(this);
                await NavigationService.Navigate<DeleteTaskDialogViewModel, TaskItemViewModel, bool>(_task);
            });

            ChangeTaskStatusCommand = new MvxAsyncCommand(async () =>
            {
                await NavigationService.Close(this);
                await NavigationService.Navigate<ChangeTaskStatusDialogViewModel, TaskItemViewModel, bool>(_task);
            });

            AddSubTaskCommand = new MvxAsyncCommand(async () =>
            {
                var parameter = AddSubTaskDialogViewModelParameter.Instance(_taskList.Id, _task);
                await NavigationService.Close(this);
                await NavigationService.Navigate<AddSubTaskDialogViewModel, AddSubTaskDialogViewModelParameter, bool>(parameter);
            });

            MoveTaskCommand = new MvxAsyncCommand(async () =>
            {
                var parameter = MoveToTaskListDialogViewModelParameter.Instance(_taskList, _task);
                await NavigationService.Close(this);
                await NavigationService.Navigate<MoveToTaskListDialogViewModel, MoveToTaskListDialogViewModelParameter, bool>(parameter);
            });

            AddReminderCommand = new MvxAsyncCommand(async () =>
            {
                var parameter = TaskDateViewModelParameter.Instance(_taskList, _task, Core.Enums.TaskNotificationDateType.REMINDER_DATE);
                await NavigationService.Close(this);
                await NavigationService.Navigate<TaskDateDialogViewModel, TaskDateViewModelParameter, bool>(parameter);
            });
        }
    }
}