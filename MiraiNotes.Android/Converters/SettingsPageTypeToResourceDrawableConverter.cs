using MiraiNotes.Core.Enums;
using MvvmCross.Converters;
using System;
using System.Globalization;

namespace MiraiNotes.Android.Converters
{
    public class SettingsPageTypeToResourceDrawableConverter : MvxValueConverter<SettingsPageType, int>
    {
        protected override int Convert(SettingsPageType value, Type targetType, object parameter, CultureInfo culture)
        {
            int resId;
            switch (value)
            {
                case SettingsPageType.GENERAL:
                    resId = Resource.Drawable.ic_home_24dp;
                    break;
                case SettingsPageType.SYNCHRONIZATION:
                    resId = Resource.Drawable.ic_sync_black_24dp;
                    break;
                case SettingsPageType.NOTIFICATIONS:
                    resId = Resource.Drawable.ic_notifications_black_24dp;
                    break;
                case SettingsPageType.ABOUT:
                    resId = Resource.Drawable.ic_info_outline_black_24dp;
                    break;
                case SettingsPageType.HOME:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return resId;
        }

    }
}