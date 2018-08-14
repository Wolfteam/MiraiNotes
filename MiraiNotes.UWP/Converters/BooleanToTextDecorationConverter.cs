using System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MiraiNotes.UWP.Converters
{
    public class BooleanToTextDecorationConverter : IValueConverter
    {
        public TextDecorations OnTrue { get; set; }
        public TextDecorations OnFalse { get; set; }

        public BooleanToTextDecorationConverter()
        {
            OnTrue = TextDecorations.None;
            OnFalse = TextDecorations.Strikethrough;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var v = (bool)value;

            return v ? OnTrue : OnFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is TextDecorations == false)
                return DependencyProperty.UnsetValue;

            if ((TextDecorations)value == OnTrue)
                return true;
            else
                return false;
        }
    }
}
