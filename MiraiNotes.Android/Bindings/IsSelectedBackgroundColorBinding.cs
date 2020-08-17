using Android.Graphics;
using Android.Util;
using Android.Views;
using AndroidX.Core.Content;
using MvvmCross.Platforms.Android.Binding.Target;
using System;

namespace MiraiNotes.Android.Bindings
{
    public class IsSelectedBackgroundColorBinding : MvxAndroidTargetBinding
    {
        public static string PropertyName => "IsSelected";
        public override Type TargetType => typeof(bool);
        public IsSelectedBackgroundColorBinding(object target) : base(target)
        {
        }

        protected override void SetValueImpl(object target, object value)
        {
            var layout = (View)target;
            bool isSelected = (bool)value;
            if (isSelected)
            {
                var typedValue = new TypedValue();
                layout.Context.Theme.ResolveAttribute(Resource.Attribute.AccentColor, typedValue, true);
                var data = typedValue.ResourceId;
                var color = ContextCompat.GetColor(layout.Context, data);
                layout.SetBackgroundColor(new Color(color));
            }
            else
            {
                layout.SetBackgroundColor(Color.Transparent);
            }
        }
    }
}