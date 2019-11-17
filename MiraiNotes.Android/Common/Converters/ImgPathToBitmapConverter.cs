using Android.Graphics;
using MiraiNotes.Android.Common.Utils;
using MvvmCross.Converters;
using System;
using System.Globalization;

namespace MiraiNotes.Android.Common.Converters
{
    public class ImgPathToBitmapConverter : MvxValueConverter<string, Bitmap>
    {
        protected override Bitmap Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            return MiscellaneousUtils.GetImageBitmap(value);
        }
    }
}