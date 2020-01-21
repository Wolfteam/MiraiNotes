using Android.Widget;
using MvvmCross.Platforms.Android.Binding.Target;
using System;

namespace MiraiNotes.Android.Bindings
{
    public class SubTaskButtonAnimateBinding : MvxAndroidTargetBinding
    {
        public static string PropertyName => "ShowSubTasks";
        public override Type TargetType => typeof(bool);

        public const float Degree180 = 180;
        public const float MinusDegree180 = -180;

        public SubTaskButtonAnimateBinding(ImageButton target) : base(target)
        {
        }

        protected override void SetValueImpl(object target, object value)
        {
            var button = target as ImageButton;
            bool showSubTasks = (bool)value;
            float degree = showSubTasks
                ? Degree180
                : MinusDegree180;

            if (button.Rotation == degree)
                return;

            if (button.Rotation == 0 && !showSubTasks)
                return;

            button.Animate().RotationBy(degree).SetDuration(250).Start();
        }
    }
}