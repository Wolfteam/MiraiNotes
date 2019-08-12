using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using MiraiNotes.Core.Enums;

namespace MiraiNotes.UWP.Converters
{
    public class TaskSortTypeToVisibilityConverter : IValueConverter
    {
        public Visibility OnTrue { get; set; }
        public Visibility OnFalse { get; set; }

        public TaskSortTypeToVisibilityConverter()
        {
            OnFalse = Visibility.Collapsed;
            OnTrue = Visibility.Visible;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null || parameter is null)
                return Visibility.Collapsed;

            var currentOrder = (TaskSortType)value;
            var targetOrder = (TaskSortType)parameter;

            return currentOrder == targetOrder ? OnTrue : OnFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility == false)
                return DependencyProperty.UnsetValue;

            if ((Visibility)value == OnTrue)
                return true;
            else
                return false;
        }
    }
}
