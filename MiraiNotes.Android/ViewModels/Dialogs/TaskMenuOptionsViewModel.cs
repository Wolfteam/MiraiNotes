using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class TaskMenuOptionsViewModel : BaseViewModel<TaskItemViewModel>
    {
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
            IAppSettingsService appSettings)
            : base(textProvider, messenger, logger.ForContext<TaskMenuOptionsViewModel>(), navigationService, appSettings)
        {
            SetCommands();
        }

        public override void Prepare(TaskItemViewModel parameter)
        {
            _task = parameter;
            string statusMessage =
                $"{(_task.IsCompleted ? GetText("Incompleted") : GetText("Completed"))}";
            MarkAsTitle = GetText("MarkTaskAs", statusMessage);
        }

        private void SetCommands()
        {
            DeleteTaskCommand = new MvxAsyncCommand(async() =>
            {
                await NavigationService.Close(this);
                await NavigationService.Navigate<DeleteTaskDialogViewModel, TaskItemViewModel, bool>(_task);
            });

            ChangeTaskStatusCommand = new MvxAsyncCommand(async () =>
            {
                await NavigationService.Close(this);
                await NavigationService.Navigate<ChangeTaskStatusDialogViewModel, TaskItemViewModel, bool>(_task);
            });
        }
    }
}