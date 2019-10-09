using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public abstract class BaseConfirmationDialogViewModel<TParameter, TResult> 
        : BaseViewModel<TParameter, TResult>, IBaseViewModel
    {
        private string _contentText;
        private string _okText;
        private string _cancelText;

        public string ContentText
        {
            get => _contentText;
            set => SetProperty(ref _contentText, value);
        }

        public string OkText
        {
            get => _okText;
            set => SetProperty(ref _okText, value);
        }

        public string CancelText
        {
            get => _cancelText;
            set => SetProperty(ref _cancelText, value);
        }

        public IMvxAsyncCommand OkCommand { get; set; }
        public IMvxAsyncCommand CloseCommand { get; set; }


        public BaseConfirmationDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger, navigationService, appSettings, telemetryService)
        {
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            Messenger.Publish(new HideKeyboardMsg(this));
            base.ViewDestroy(viewFinishing);
        }
    }
}