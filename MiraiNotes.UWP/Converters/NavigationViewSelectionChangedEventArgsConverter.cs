using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace MiraiNotes.UWP.Converters
{
    public class NavigationViewSelectionChangedEventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var args = (NavigationViewSelectionChangedEventArgs)value;
            return args.SelectedItem;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
