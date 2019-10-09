using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Parameters;
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
    public class MoveTaskDialogViewModel : BaseConfirmationDialogViewModel<MoveTaskDialogViewModelParameter, bool>
    {
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;

        public MoveTaskDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            ITelemetryService telemetryService,
            IAppSettingsService appSettings,
            IMiraiNotesDataService dataService,
            IDialogService dialogService)
            : base(textProvider, messenger, logger.ForContext<MoveTaskDialogViewModel>(), navigationService, appSettings, telemetryService)
        {
            _dataService = dataService;
            _dialogService = dialogService;
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
                .MoveAsync(selectedTaskList.Id, task.TaskID, task.ParentTask, task.Position);

            if (moveResponse.Succeed && task.HasSubTasks)
            {
                foreach (var st in task.SubTasks)
                {
                    st.ParentTask = moveResponse.Result.GoogleTaskID;
                }

                await MoveSubTasksAsync(selectedTaskList.Id, task.SubTasks);
            }

            if (!moveResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(MoveCurrentTask)}: An error occurred while tryingg to move taskId = {task.ID}. " +
                    $"Error = {moveResponse.Message}");
            }
            else
            {
                Messenger.Publish(new TaskMovedMsg(this, task.TaskID, selectedTaskList.Id, task.ParentTask));
                _dialogService.ShowSnackBar(GetText("TaskWasMoved", Parameter.CurrentTaskList.Title, selectedTaskList.Title));
            }

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            await NavigationService.Close(this, moveResponse.Succeed);
        }


        public async Task MoveSubTasksAsync(string taskListID, MvxObservableCollection<TaskItemViewModel> subTasks)
        {
            //TODO: CREO QUE ACA NO SE ESTA COLOCANDO EN LA POSICION CORRECTA
            var stList = new List<string>();
            foreach (var st in subTasks)
            {
                var moveResponse = await _dataService
                    .TaskService
                    .MoveAsync(taskListID, st.TaskID, st.ParentTask, stList.LastOrDefault());
                if (moveResponse.Succeed)
                {
                    stList.Add(moveResponse.Result.GoogleTaskID);
                }
                else
                {
                    Logger.Error(
                        $"{nameof(MoveSubTasksAsync)}: An error occurred while trying to move subtaskId = {st.TaskID}. " +
                        $"Error = {moveResponse.Message}");
                }
            }
        }

    }
}