using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;
using System.Collections.Generic;

namespace MiraiNotes.Android.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel, IBaseViewModel
    {
        #region Members
        public List<MvxSubscriptionToken> SubscriptionTokens = new List<MvxSubscriptionToken>();
        private string _title = string.Empty;
        #endregion

        #region Properties
        public ITextProvider TextProvider { get; }
        public IMvxMessenger Messenger { get; }
        public ILogger Logger { get; }
        public IMvxNavigationService NavigationService { get; }
        public IAppSettingsService AppSettings { get; }
        public ITelemetryService TelemetryService { get; }

        public AppThemeType CurrentAppTheme
            => AppSettings.AppTheme;
        public string CurrentHexAccentColor
            => AppSettings.AppHexAccentColor;
        public AppLanguageType CurrentAppLanguge
            => AppSettings.AppLanguage;
        public string this[string key]
            => TextProvider.GetText(string.Empty, string.Empty, key);

        /// <summary>
        /// This one is to set the apptollbar title
        /// you can only use it in the vm whoose view contains the 
        /// toolbar
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        #endregion

        public BaseViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService)
        {
            TextProvider = textProvider;
            Messenger = messenger;
            Logger = logger;
            NavigationService = navigationService;
            AppSettings = appSettings;
            TelemetryService = telemetryService;

            RegisterMessages();
            SetCommands();
        }

        public virtual void SetCommands()
        {
        }

        public virtual void RegisterMessages()
        {
        }

        public string GetText(string key)
            => TextProvider.GetText(string.Empty, string.Empty, key);

        public string GetText(string key, params string[] args)
            => TextProvider.GetText(string.Empty, string.Empty, key, args);

        public override void ViewDestroy(bool viewFinishing = true)
        {
            base.ViewDestroy(viewFinishing);
            foreach (var token in SubscriptionTokens)
            {
                token.Dispose();
            }
        }
    }

    public abstract class BaseViewModel<TParameter> : MvxViewModel<TParameter>, IBaseViewModel
    {
        #region Members
        public List<MvxSubscriptionToken> SubscriptionTokens = new List<MvxSubscriptionToken>();
        private string _title;
        #endregion

        #region Properties
        public ITextProvider TextProvider { get; }
        public IMvxMessenger Messenger { get; }
        public ILogger Logger { get; }
        public IMvxNavigationService NavigationService { get; }
        public IAppSettingsService AppSettings { get; }
        public ITelemetryService TelemetryService { get; }

        public AppThemeType CurrentAppTheme
            => AppSettings.AppTheme;
        public string CurrentHexAccentColor
            => AppSettings.AppHexAccentColor;
        public AppLanguageType CurrentAppLanguge
            => AppSettings.AppLanguage;
        public string this[string key]
            => TextProvider.GetText(string.Empty, string.Empty, key);
        public TParameter Parameter { get; private set; }

        /// <summary>
        /// This one is to set the apptollbar title
        /// you can only use it in the vm whoose view contains the 
        /// toolbar
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        #endregion

        public BaseViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService)
        {
            TextProvider = textProvider;
            Messenger = messenger;
            Logger = logger;
            NavigationService = navigationService;
            AppSettings = appSettings;
            TelemetryService = telemetryService;

            RegisterMessages();
            SetCommands();
        }
        public virtual void SetCommands()
        {
        }

        public virtual void RegisterMessages()
        {
        }

        public string GetText(string key)
            => TextProvider.GetText(string.Empty, string.Empty, key);

        public string GetText(string key, params string[] args)
            => TextProvider.GetText(string.Empty, string.Empty, key, args);

        public override void Prepare(TParameter parameter)
        {
            Parameter = parameter;
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
        : MvxViewModel<TParameter, TResult>, IMvxViewModel<TParameter, TResult>, IBaseViewModel
    {
        #region Members
        public List<MvxSubscriptionToken> SubscriptionTokens = new List<MvxSubscriptionToken>();
        private string _title;
        #endregion

        #region Properties
        public ITextProvider TextProvider { get; }
        public IMvxMessenger Messenger { get; }
        public ILogger Logger { get; }
        public IMvxNavigationService NavigationService { get; }
        public IAppSettingsService AppSettings { get; }
        public ITelemetryService TelemetryService { get; }

        public AppThemeType CurrentAppTheme
            => AppSettings.AppTheme;
        public string CurrentHexAccentColor
            => AppSettings.AppHexAccentColor;
        public AppLanguageType CurrentAppLanguge
            => AppSettings.AppLanguage;
        public string this[string key]
            => TextProvider.GetText(string.Empty, string.Empty, key);
        public TParameter Parameter { get; private set; }


        /// <summary>
        /// This one is to set the apptollbar title
        /// you can only use it in the vm whoose view contains the 
        /// toolbar
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        #endregion

        public BaseViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService)
        {
            TextProvider = textProvider;
            Messenger = messenger;
            Logger = logger;
            NavigationService = navigationService;
            AppSettings = appSettings;
            TelemetryService = telemetryService;

            RegisterMessages();
            SetCommands();
        }

        public virtual void SetCommands()
        {
        }

        public virtual void RegisterMessages()
        {
        }

        public string GetText(string key)
            => TextProvider.GetText(string.Empty, string.Empty, key);

        public string GetText(string key, params string[] args)
            => TextProvider.GetText(string.Empty, string.Empty, key, args);

        public override void Prepare(TParameter parameter)
        {
            Parameter = parameter;
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