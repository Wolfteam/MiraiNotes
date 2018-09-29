using MiraiNotes.UWP.Utils;
using System;
using Windows.UI.Xaml.Data;

namespace MiraiNotes.UWP.Converters
{
    public class HexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null)
                return null;

            return MiscellaneousUtils.GetColor((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
