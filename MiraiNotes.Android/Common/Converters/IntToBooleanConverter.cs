using System;
using System.Globalization;
using MvvmCross.Converters;

namespace MiraiNotes.Android.Common.Converters
{
    public class IntToBooleanConverter : MvxValueConverter<int, bool>
    {
        protected override bool Convert(int value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverted = (bool)parameter;

            if (inverted)
                return value == 0;
            return value > 0;
        }
    }
}