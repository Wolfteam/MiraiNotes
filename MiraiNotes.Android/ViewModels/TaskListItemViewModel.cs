using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System;

namespace MiraiNotes.Android.ViewModels
{
    public class TaskListItemViewModel : BaseViewModel
    {
        private string _googleId;
        private string _title;
        private DateTimeOffset? _updatedAt;
        private int _numberOfTasks;
        private bool _isSelected;

        public int Id { get; set; }

        public string GoogleId
        {
            get => _googleId;
            set => SetProperty(ref _googleId, value);
        }

        public new string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public DateTimeOffset? UpdatedAt
        {
            get => _updatedAt;
            set => SetProperty(ref _updatedAt, value);
        }

        //you need to manually set this one
        public int NumberOfTasks
        {
            get => _numberOfTasks;
            set => SetProperty(ref _numberOfTasks, value);
        }

        //you need to manually set this one
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public IMvxCommand EditTaskListCommand { get; private set; }
        public IMvxCommand DeleteTaskListCommand { get; private set; }

        public TaskListItemViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<TaskListItemViewModel>(), navigationService, appSettings, telemetryService)
        {
        }

        public override void SetCommands()
        {
            base.SetCommands();
            EditTaskListCommand = new MvxCommand(
                () => Messenger.Publish(new OnManageTaskListItemClickMsg(this, this, false, true)));

            DeleteTaskListCommand = new MvxCommand(
                () => Messenger.Publish(new OnManageTaskListItemClickMsg(this, this, true, false)));
        }
    }
}