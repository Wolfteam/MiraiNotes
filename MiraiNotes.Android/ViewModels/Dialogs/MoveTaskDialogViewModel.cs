using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Models;
using MiraiNotes.Shared.Helpers;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class MoveTaskDialogViewModel : BaseConfirmationDialogViewModel<MoveTaskDialogViewModelParameter, bool>
    {
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;

        public MoveTaskDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            ITelemetryService telemetryService,
            IAppSettingsService appSettings,
            IMiraiNotesDataService dataService,
            IDialogService dialogService,
            INotificationService notificationService)
            : base(textProvider, messenger, logger.ForContext<MoveTaskDialogViewModel>(), navigationService, appSettings, telemetryService)
        {
            _dataService = dataService;
            _dialogService = dialogService;
            _notificationService = notificationService;
        }

        public override void Prepare(MoveTaskDialogViewModelParameter parameter)
        {
            base.Prepare(parameter);
            Title = GetText("Confirmation");
            ContentText = GetText("MoveTaskConfirmationB", Parameter.Task.Title, Parameter.NewTaskList.Title);
            OkText = GetText("Yes");
            CancelText = GetText("No");
        }

        public override void SetCommands()
        {
            base.SetCommands();

            OkCommand = new MvxAsyncCommand(MoveCurrentTask);
            CloseCommand = new MvxAsyncCommand(() => NavigationService.Close(this, false));
        }

        private async Task MoveCurrentTask()
        {
            Messenger.Publish(new ShowProgressOverlayMsg(this));
            var selectedTaskList = Parameter.NewTaskList;
            var task = Parameter.Task;

            var moveResponse = await _dataService
                .TaskService
                .MoveAsync(selectedTaskList.GoogleId, task.GoogleId, null, null);

            if (moveResponse.Succeed && task.HasSubTasks)
            {
                var movedTask = moveResponse.Result;

                //If this task had a reminder, we need to recreate it
                if (TasksHelper.HasReminderId(movedTask.RemindOnGUID, out int id))
                {
                    ReAddReminderDate(id, selectedTaskList, movedTask);
                }

                foreach (var st in task.SubTasks)
                {
                    st.ParentTask = moveResponse.Result.GoogleTaskID;
                }

                await MoveSubTasksAsync(selectedTaskList.GoogleId, task.SubTasks);
            }

            if (!moveResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(MoveCurrentTask)}: An error occurred while tryingg to move taskId = {task.Id}. " +
                    $"Error = {moveResponse.Message}");
            }
            else
            {
                Messenger.Publish(new TaskMovedMsg(this, task.GoogleId, selectedTaskList.GoogleId, task.ParentTask));
                _dialogService.ShowSnackBar(GetText("TaskWasMoved", Parameter.CurrentTaskList.Title, selectedTaskList.Title));
            }

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            await NavigationService.Close(this, moveResponse.Succeed);
        }


        public async Task MoveSubTasksAsync(string taskListID, MvxObservableCollection<TaskItemViewModel> subTasks)
        {
            var orderedSubTasks = subTasks.OrderBy(st => st.Position).ToList();
            foreach (var st in orderedSubTasks)
            {
                var moveResponse = await _dataService
                    .TaskService
                    .MoveAsync(taskListID, st.GoogleId, st.ParentTask, null);
                if (!moveResponse.Succeed)
                {
                    Logger.Error(
                        $"{nameof(MoveSubTasksAsync)}: An error occurred while trying to move subtaskId = {st.GoogleId}. " +
                        $"Error = {moveResponse.Message}");
                }
                else
                {
                    var movedTask = moveResponse.Result;
                    if (TasksHelper.HasReminderId(movedTask.RemindOnGUID, out int id))
                    {
                        ReAddReminderDate(id, Parameter.NewTaskList, movedTask);
                    }
                }
            }
        }


        private void ReAddReminderDate(
            int notificationId,
            TaskListItemViewModel taskList,
            GoogleTask task)
        {
            string notes = TasksHelper.GetNotesForNotification(task.Notes);
            _notificationService.RemoveScheduledNotification(notificationId);
            _notificationService.ScheduleNotification(new TaskReminderNotification
            {
                Id = notificationId,
                TaskListId = taskList.GoogleId,
                TaskId = task.GoogleTaskID,
                TaskListTitle = taskList.Title,
                TaskTitle = task.Title,
                TaskBody = notes,
                DeliveryOn = task.RemindOn.Value
            });
        }
    }
}