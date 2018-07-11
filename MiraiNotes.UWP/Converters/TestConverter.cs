using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace MiraiNotes.UWP.Converters
{
    public class TestConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            // value = IsPlaying
            if (value is bool b)
            {
                return b;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            var algo = (SelectionChangedEventArgs)value;
            //algo.AddedItems;
            return value; // Needed because IsPlaying is twoway bound to IsSelected.
        }
    }
}
