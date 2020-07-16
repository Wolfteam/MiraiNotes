using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using AndroidX.Core.View;
using Java.Lang;
using AttributeSet = Android.Util.IAttributeSet;

namespace MiraiNotes.Android.Common.Utils
{
    /// <summary>
    /// Basically this one should be used only if you want toslide up the FAB whenever the seekbar appear
    /// https://github.com/ajaydewari/FloatingActionButtonMenu/blob/master/app/src/main/java/com/ajaysinghdewari/floatingactionbuttonmenu/activities/utils/MoveUpwardBehavior.java
    /// </summary>
    public class MoveUpwardBehavior : CoordinatorLayout.Behavior
    {

        public MoveUpwardBehavior() : base()
        {
            
        }

        public MoveUpwardBehavior(Context context, AttributeSet attrs) : base(context, attrs)
        {

        }

        public override bool LayoutDependsOn(CoordinatorLayout parent, Object child, View dependency)
        {
            return dependency is Snackbar.SnackbarLayout;
        }

        public override bool OnDependentViewChanged(CoordinatorLayout parent, Object child, View dependency)
        {
            float translationY = Math.Min(0, dependency.TranslationY - dependency.Height);
            ((View)child).TranslationY = translationY;
            return true;
        }

        public override void OnDependentViewRemoved(CoordinatorLayout parent, Object child, View dependency)
        {
            ViewCompat.Animate((View)child).TranslationY(0).Start();
        }
    }
}