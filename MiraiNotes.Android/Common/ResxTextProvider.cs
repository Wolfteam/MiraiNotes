﻿using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Plugin.ResxLocalization;
using System.Globalization;
using System.Resources;

namespace MiraiNotes.Android
{
    public class ResxTextProvider : MvxResxTextProvider, ITextProvider
    {
        private readonly IMvxMessenger _messenger;

        public ResxTextProvider(
            ResourceManager resourceManager,
            IMvxMessenger messenger)
            : base(resourceManager)
        {
            _messenger = messenger;
        }

        public string Get(string key)
        {
            return GetText(string.Empty, string.Empty, key);
        }

        public string Get(string key, params string[] formatArgs)
        {
            return GetText(string.Empty, string.Empty, key, formatArgs);
        }

        public void SetLanguage(AppLanguageType appLanguage, bool restartActivity = true)
        {
            string lang = appLanguage == AppLanguageType.English
                ? "en"
                : "es";

            CurrentLanguage = new CultureInfo(lang);
            //let all ViewModels that are active know that the culture has changed
            _messenger.Publish(new AppLanguageChangedMessage(this, appLanguage, restartActivity));
        }
    }
}