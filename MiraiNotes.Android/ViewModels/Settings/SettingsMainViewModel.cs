using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;

namespace MiraiNotes.Android.ViewModels.Settings
{
    public class SettingsMainViewModel : BaseViewModel
    {
        public IMvxAsyncCommand InitViewCommand { get; private set; }

        public SettingsMainViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings)
            : base(textProvider, messenger, logger.ForContext<SettingsMainViewModel>(), navigationService, appSettings)
        {
            SetCommands();
            RegisterMessages();
        }

        private void SetCommands()
        {
            InitViewCommand = new MvxAsyncCommand
                (async () => await NavigationService.Navigate<SettingsHomeViewModel>());
        }

        private void RegisterMessages()
        {
            var tokens = new[]
            {
                Messenger.Subscribe<SettingsTitleChanged>(msg => Title = msg.NewTitle)
            };

            SubscriptionTokens.AddRange(tokens);
        }
    }
}