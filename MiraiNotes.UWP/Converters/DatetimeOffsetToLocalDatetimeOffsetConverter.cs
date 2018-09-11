using System;
using Windows.UI.Xaml.Data;

namespace MiraiNotes.UWP.Converters
{
    public class DatetimeOffsetToLocalDatetimeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                return null;

            //A datetime offset for example 2018-09-30 00:00:00+00:00
            //if my local time zone is -4 then the above date will become 2018-09-29 XXXXX (CalendarDatePicker will do this)
            //but i dont want that, instead i want to keep the same year, month and day but with my local time
            var date = (DateTimeOffset)value;
            var dateToUse = new DateTimeOffset(date.DateTime, date.ToLocalTime().Offset);
            return dateToUse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                return value;
            var date = (DateTimeOffset)value;
            return new DateTimeOffset(date.DateTime, TimeSpan.Zero);
        }
    }
}
