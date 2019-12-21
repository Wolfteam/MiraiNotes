using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Android.Models.Results;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Models;
using MiraiNotes.Shared.Helpers;
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
    public class MoveTaskDialogViewModel 
        : BaseConfirmationDialogViewModel<MoveTaskDialogViewModelParameter, MoveTaskDialogViewModelResult>
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
            if (!Parameter.IsMultipleTasks)
                ContentText = GetText("MoveTaskConfirmationB", Parameter.Task.Title, Parameter.NewTaskList.Title);
            else
                ContentText = GetText("MoveTaskConfirmationC", GetText("XTasks", $"{Parameter.Tasks.Count}"), Parameter.NewTaskList.Title);
            OkText = GetText("Yes");
            CancelText = GetText("No");
        }

        public override void SetCommands()
        {
            base.SetCommands();

            OkCommand = new MvxAsyncCommand(MoveTask);
            CloseCommand = new MvxAsyncCommand(() => NavigationService.Close(this, MoveTaskDialogViewModelResult.Nothing()));
        }

        private async Task MoveTask()
        {
            var selectedTaskList = Parameter.NewTaskList;
            var results = new List<bool>();
            if (Parameter.IsMultipleTasks)
            {
                foreach (var task in Parameter.Tasks)
                {
                    bool result = await MoveTask(selectedTaskList, task);
                    results.Add(result);
                }
            }
            else
            {
                bool result = await MoveTask(selectedTaskList, Parameter.Task);
                results.Add(result);
            }

            if (results.Any(r => !r))
            {
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                var result = Parameter.IsMultipleTasks
                    ? MoveTaskDialogViewModelResult.Partial()
                    : MoveTaskDialogViewModelResult.Moved(false);

                await NavigationService.Close(this, result);
            }
            else
            {
                if (!Parameter.IsMultipleTasks)
                    _dialogService.ShowSnackBar(GetText("TaskWasMoved", Parameter.CurrentTaskList.Title, selectedTaskList.Title));
                else
                    _dialogService.ShowSnackBar(GetText("SelectedTasksWereMoved", selectedTaskList.Title));
                var result = MoveTaskDialogViewModelResult.Moved(true);
                await NavigationService.Close(this, result);
            }
        }

        private async Task<bool> MoveTask(TaskListItemViewModel selectedTaskList ,TaskItemViewModel task)
        {
            Messenger.Publish(new ShowProgressOverlayMsg(this));

            var moveResponse = await _dataService
                .TaskService
                .MoveAsync(selectedTaskList.GoogleId, task.GoogleId, null, null);

            if (moveResponse.Succeed)
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

                await MoveSubTasks(selectedTaskList.GoogleId, task.SubTasks);
            }
            else
            {
                Logger.Error(
                    $"{nameof(MoveTask)}: An error occurred while tryingg to move taskId = {task.Id}. " +
                    $"Error = {moveResponse.Message}");
            }

            Messenger.Publish(new TaskMovedMsg(this, task.GoogleId, selectedTaskList.GoogleId, task.ParentTask));
            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            return moveResponse.Succeed;
        }

        public async Task MoveSubTasks(string taskListID, MvxObservableCollection<TaskItemViewModel> subTasks)
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
                        $"{nameof(MoveSubTasks)}: An error occurred while trying to move subtaskId = {st.GoogleId}. " +
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
            if (!TasksHelper.CanReAddReminder(task.RemindOn.Value))
            {
                return;
            }

            string notes = TasksHelper.GetNotesForNotification(task.Notes);
            _notificationService.RemoveScheduledNotification(notificationId);
            _notificationService.ScheduleNotification(new TaskReminderNotification
            {
                Id = notificationId,
                TaskListId = taskList.Id,
                TaskId = task.ID,
                TaskListTitle = taskList.Title,
                TaskTitle = task.Title,
                TaskBody = notes,
                DeliveryOn = task.RemindOn.Value
            });
        }
    }
}