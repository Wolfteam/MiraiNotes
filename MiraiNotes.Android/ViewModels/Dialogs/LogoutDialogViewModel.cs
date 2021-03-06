﻿using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class LogoutDialogViewModel : BaseConfirmationDialogViewModel<bool?, bool>
    {
        public LogoutDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<LogoutDialogViewModel>(), navigationService, appSettings, telemetryService)
        {
        }

        public override void Prepare(bool? parameter)
        {
            base.Prepare(parameter);

            Title = $"{GetText("Confirmation")}";
            ContentText = GetText("WannaLogOut");
            OkText = GetText("Yes");
            CancelText = GetText("No");
        }

        public override void SetCommands()
        {
            base.SetCommands();
            OkCommand = new MvxAsyncCommand(() => NavigationService.Close(this, true));
            CloseCommand = new MvxAsyncCommand(() => NavigationService.Close(this, false));
        }
    }
}