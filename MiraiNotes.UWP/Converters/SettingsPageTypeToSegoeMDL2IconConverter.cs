using MiraiNotes.UWP.Models;
using System;
using Windows.UI.Xaml.Data;

namespace MiraiNotes.UWP.Converters
{
    public class SettingsPageTypeToSegoeMDL2IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(SettingsPageType), "The settings page type cannot be null");

            var settingPageType = (SettingsPageType)value;
            switch (settingPageType)
            {
                case SettingsPageType.GENERAL:
                    return "\uE80F";
                case SettingsPageType.ACCOUNT:
                    return "\uE77B";
                case SettingsPageType.SYNCHRONIZATION:
                    return "\uE895";
                case SettingsPageType.NOTIFICATIONS:
                    return "\uE7E7";
                case SettingsPageType.ABOUT:
                    return "\uE946";
                default:
                    throw new ArgumentOutOfRangeException(nameof(SettingsPageType), settingPageType, "The setting page type doesnt exists");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
