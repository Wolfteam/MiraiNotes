using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Android.Models.Results;
using MiraiNotes.Shared.Helpers;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class DeleteTaskDialogViewModel 
        : BaseConfirmationDialogViewModel<DeleteTaskDialogViewModelParameter, DeleteTaskDialogViewModelResult>
    {
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;

        public DeleteTaskDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService,
            IMiraiNotesDataService dataService,
            IDialogService dialogService,
            INotificationService notificationService)
            : base(textProvider, messenger, logger.ForContext<DeleteTaskDialogViewModel>(), navigationService, appSettings, telemetryService)
        {
            _dataService = dataService;
            _dialogService = dialogService;
            _notificationService = notificationService;
        }

        public override void Prepare(DeleteTaskDialogViewModelParameter parameter)
        {
            base.Prepare(parameter);

            if (!parameter.IsMultipleTasks)
            {
                Title = GetText("Confirmation");
                ContentText = GetText("DeleteTaskConfirmation", Parameter.Task.Title);
            }
            else
            {
                Title = $"{GetText("Confirmation")} - {GetText("DeleteXTasks", $"{parameter.Tasks.Count}")}";
                ContentText = GetText("DeleteMultipleTasksConfirmation");
            }
            OkText = GetText("Yes");
            CancelText = GetText("No");
        }

        public override void SetCommands()
        {
            base.SetCommands();
            OkCommand = new MvxAsyncCommand(DeleteMultipleTasks);
            CloseCommand = new MvxAsyncCommand(() => NavigationService.Close(this, DeleteTaskDialogViewModelResult.Nothing()));
        }

        private async Task<bool> DeleteTask(TaskItemViewModel task)
        {
            if (task.HasParentTask && task.IsNew ||
                task.IsNew)
            {
                Messenger.Publish(new TaskDeletedMsg(this, task.GoogleId, task.ParentTask));
                return true;
            }

            Messenger.Publish(new ShowProgressOverlayMsg(this));

            var deleteResponse = await _dataService
                .TaskService
                .RemoveTaskAsync(task.GoogleId);

            if (TasksHelper.HasReminderId(task.RemindOnGUID, out int id))
            {
                _notificationService.RemoveScheduledNotification(id);
            }

            if (task.HasSubTasks)
            {
                foreach (var st in task.SubTasks)
                    if (TasksHelper.HasReminderId(st.RemindOnGUID, out int stReminderId))
                        _notificationService.RemoveScheduledNotification(stReminderId);
            }

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            if (!deleteResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(DeleteTask)}: Couldn't delete the selected task." +
                    $"Error = {deleteResponse.Message}");
                return false;
            }

            Messenger.Publish(new TaskDeletedMsg(this, task.GoogleId, task.ParentTask));
            return true;
        }

        private async Task DeleteMultipleTasks()
        {
            var results = new List<bool>();
            if (Parameter.IsMultipleTasks)
            {
                foreach (var task in Parameter.Tasks)
                {
                    bool result = await DeleteTask(task);
                    results.Add(result);
                }
            }
            else
            {
                bool result = await DeleteTask(Parameter.Task);
                results.Add(result);
            }

            if (results.Any(r => !r))
            {
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                var result = Parameter.IsMultipleTasks
                    ? DeleteTaskDialogViewModelResult.Partial()
                    : DeleteTaskDialogViewModelResult.Deleted(false);

                await NavigationService.Close(this, result);
            }
            else
            {
                var result = DeleteTaskDialogViewModelResult.Deleted(true);
                await NavigationService.Close(this, result);
            }
        }
    }
}