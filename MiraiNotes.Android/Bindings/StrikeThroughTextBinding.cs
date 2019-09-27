using Android.Graphics;
using Android.Widget;
using MvvmCross.Platforms.Android.Binding.Target;
using System;

namespace MiraiNotes.Android.Bindings
{
    public class StrikeThroughTextBinding : MvxAndroidTargetBinding
    {
        public static string PropertyName => "StrikeThroughText";
        public override Type TargetType => typeof(bool);

        public StrikeThroughTextBinding(object target) : base(target)
        {
        }

        protected override void SetValueImpl(object target, object value)
        {
            var textView = (TextView)target;
            bool strikeThrough = (bool)value;

            if (strikeThrough)
                textView.PaintFlags |= PaintFlags.StrikeThruText;
            else
                textView.PaintFlags &= ~PaintFlags.StrikeThruText;
        }
    }
}