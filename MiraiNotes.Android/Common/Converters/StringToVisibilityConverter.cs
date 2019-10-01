using MvvmCross.Plugin.Visibility;
using MvvmCross.UI;
using System.Globalization;

namespace MiraiNotes.Android.Common.Converters
{
    public class StringToVisibilityConverter : MvxBaseVisibilityValueConverter<string>
    {
        protected override MvxVisibility Convert(string value, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value))
                return MvxVisibility.Collapsed;
            return MvxVisibility.Visible;
        }
    }
}