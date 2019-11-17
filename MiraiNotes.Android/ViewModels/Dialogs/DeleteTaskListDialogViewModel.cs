using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Enums;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class DeleteTaskListDialogViewModel : BaseConfirmationDialogViewModel<TaskListItemViewModel, bool>
    {
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;

        public DeleteTaskListDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService,
            IMiraiNotesDataService dataService,
            IDialogService dialogService)
            : base(textProvider, messenger, logger, navigationService, appSettings, telemetryService)
        {
            _dataService = dataService;
            _dialogService = dialogService;
        }

        public override void Prepare(TaskListItemViewModel parameter)
        {
            base.Prepare(parameter);
            ContentText = GetText("DeleteTaskConfirmation", parameter.Title);
        }

        public override void SetCommands()
        {
            base.SetCommands();
            OkCommand = new MvxAsyncCommand(() => DeleteTaskList(Parameter));
            CloseCommand = new MvxAsyncCommand(() => NavigationService.Close(this, false));
        }

        public async Task DeleteTaskList(TaskListItemViewModel taskList)
        {
            Messenger.Publish(new ShowProgressOverlayMsg(this));

            var dbResponse = await _dataService
                .TaskListService
                .FirstOrDefaultAsNoTrackingAsync(tl => tl.GoogleTaskListID == taskList.GoogleId);
            if (!dbResponse.Succeed)
            {
                Messenger.Publish(new ShowProgressOverlayMsg(this, false));
                Logger.Error(
                    $"{nameof(DeleteTaskList)}: Couldn't retrieve the selected task list = {taskList.Title}" +
                    $"Error = {dbResponse.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                await NavigationService.Close(this, false);
                return;
            }

            EmptyResponseDto response;
            //if the task is created but wasnt synced, we remove it from the db
            if (dbResponse.Result.LocalStatus == LocalStatus.CREATED)
            {
                response = await _dataService
                    .TaskListService
                    .RemoveAsync(dbResponse.Result);
            }
            else
            {
                dbResponse.Result.LocalStatus = LocalStatus.DELETED;
                dbResponse.Result.ToBeSynced = true;

                response = await _dataService
                    .TaskListService
                    .UpdateAsync(dbResponse.Result);
            }

            if (!response.Succeed)
            {
                Messenger.Publish(new ShowProgressOverlayMsg(this, false));
                Logger.Error(
                    $"{nameof(DeleteTaskList)}: Coudln't delete the task list = {taskList.Title}" +
                    $"Error = {dbResponse.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                await NavigationService.Close(this, false);

                return;
            }

            Messenger.Publish(new TaskListDeletedMsg(this, Parameter));

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            _dialogService.ShowSucceedToast(GetText("TaskListWasRemoved"));

            await NavigationService.Close(this, true);
        }
    }
}