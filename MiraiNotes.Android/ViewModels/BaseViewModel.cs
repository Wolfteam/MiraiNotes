using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Enums;
using MvvmCross.Localization;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System.Collections.Generic;

namespace MiraiNotes.Android.ViewModels
{
    public class BaseViewModel : MvxViewModel
    {
        private IMvxTextProvider _textProvider;
        public List<MvxSubscriptionToken> SubscriptionTokens = new List<MvxSubscriptionToken>();


        public IMvxMessenger Messenger { get; private set; }
        public IAppSettingsService AppSettings { get; private set; }
        public IMvxLanguageBinder TextSource
            => new MvxLanguageBinder(string.Empty, string.Empty);
        public AppThemeType CurrentAppTheme
            => AppSettings.AppTheme;
        public string CurrentHexAccentColor
            => AppSettings.AppHexAccentColor;
        public string this[string key]
            => _textProvider.GetText(string.Empty, string.Empty, key);


        public BaseViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IAppSettingsService appSettings)
        {
            _textProvider = textProvider;
            Messenger = messenger;
            AppSettings = appSettings;
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            base.ViewDestroy(viewFinishing);
            foreach (var token in SubscriptionTokens)
            {
                token.Dispose();
            }
        }
    }

    public abstract class BaseViewModel<TParameter> : MvxViewModel<TParameter>
    {
        private IMvxTextProvider _textProvider;
        public List<MvxSubscriptionToken> SubscriptionTokens = new List<MvxSubscriptionToken>();


        public IMvxMessenger Messenger { get; private set; }
        public IAppSettingsService AppSettings { get; private set; }
        public IMvxLanguageBinder TextSource
            => new MvxLanguageBinder(string.Empty, string.Empty);
        public AppThemeType CurrentAppTheme
            => AppSettings.AppTheme;
        public string CurrentHexAccentColor
            => AppSettings.AppHexAccentColor;
        public string this[string key]
            => _textProvider.GetText(string.Empty, string.Empty, key);


        public BaseViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IAppSettingsService appSettings)
        {
            _textProvider = textProvider;
            Messenger = messenger;
            AppSettings = appSettings;
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            base.ViewDestroy(viewFinishing);
            foreach (var token in SubscriptionTokens)
            {
                token.Dispose();
            }
        }
    }

    public abstract class BaseViewModel<TParameter, TResult>
        : MvxViewModel<TParameter, TResult>, IMvxViewModel<TParameter, TResult>
    {
        private IMvxTextProvider _textProvider;
        public List<MvxSubscriptionToken> SubscriptionTokens = new List<MvxSubscriptionToken>();


        public IMvxMessenger Messenger { get; private set; }
        public IAppSettingsService AppSettings { get; private set; }
        public IMvxLanguageBinder TextSource
            => new MvxLanguageBinder(string.Empty, string.Empty);
        public AppThemeType CurrentAppTheme
            => AppSettings.AppTheme;
        public string CurrentHexAccentColor
            => AppSettings.AppHexAccentColor;
        public string this[string key]
            => _textProvider.GetText(string.Empty, string.Empty, key);


        public BaseViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IAppSettingsService appSettings)
        {
            _textProvider = textProvider;
            Messenger = messenger;
            AppSettings = appSettings;
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            if (viewFinishing && CloseCompletionSource != null && !CloseCompletionSource.Task.IsCompleted && !CloseCompletionSource.Task.IsFaulted)
                CloseCompletionSource?.TrySetCanceled();

            base.ViewDestroy(viewFinishing);
            foreach (var token in SubscriptionTokens)
            {
                token.Dispose();
            }
        }
    }

}