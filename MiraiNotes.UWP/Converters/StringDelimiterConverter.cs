using System;
using Windows.UI.Xaml.Data;

namespace MiraiNotes.UWP.Converters
{
    public class StringDelimiterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return ".....";

            int maxCharsAllowed = int.Parse((string)(parameter ?? "0"));

            string text = ((string)value)
                .Replace("\n", " ")
                .Trim();
            return text.Length > maxCharsAllowed ?
                $"{text.Substring(0, maxCharsAllowed)}...." :
                $"{text}....";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
