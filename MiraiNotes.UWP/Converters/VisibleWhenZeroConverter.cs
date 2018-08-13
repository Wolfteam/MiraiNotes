using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MiraiNotes.UWP.Converters
{
    public class VisibleWhenZeroConverter : IValueConverter
    {
        public Visibility OnTrue { get; set; }
        public Visibility OnFalse { get; set; }

        public VisibleWhenZeroConverter()
        {
            OnFalse = Visibility.Collapsed;
            OnTrue = Visibility.Visible;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
            => Equals(0, (int)value) ? OnTrue : OnFalse;

        public object ConvertBack(object value, Type targetType, object parameter, string language) 
            => throw new NotImplementedException();
    }
}
