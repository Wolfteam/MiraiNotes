using Android.Animation;
using Android.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.Target;
using System;

namespace MiraiNotes.Android.Bindings
{
    public class ExpandCollapseSubTasksAnimateBinding : MvxAndroidTargetBinding
    {
        public static string PropertyName => "ExpandCollapseSubTasks";
        public override Type TargetType => typeof(bool);

        public ExpandCollapseSubTasksAnimateBinding(MvxRecyclerView target) : base(target)
        {
        }

        protected override void SetValueImpl(object target, object value)
        {
            if (!(target is MvxRecyclerView recyclerView))
                return;

            if (recyclerView.ItemsSource is null)
                return;

            bool show = (bool)value;

            if (recyclerView.Visibility == ViewStates.Gone && show)
            {
                recyclerView.Visibility = ViewStates.Visible;
                int widthSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                int heightSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                recyclerView.Measure(widthSpec, heightSpec);

                var mAnimator = BuildAnimator(0, recyclerView.MeasuredHeight, recyclerView);
                mAnimator.Start();

                //var manager = recyclerView.GetLayoutManager() as LinearLayoutManager;
                //int distance;
                //View first = recyclerView.GetChildAt(0);
                //int height = first.Height;
                //int current = recyclerView.GetChildAdapterPosition(first);
                //int p = Math.Abs(position - current);
                //if (p > 5) distance = (p - (p - 5)) * height;
                //else distance = p * height;
                //manager.ScrollToPositionWithOffset(position, distance);
            }

            if (recyclerView.Visibility == ViewStates.Visible && !show)
            {
                int finalHeight = recyclerView.Height;
                var mAnimator = BuildAnimator(finalHeight, 0, recyclerView);
                mAnimator.Start();
                mAnimator.AnimationEnd += (object IntentSender, EventArgs arg) =>
                {
                    recyclerView.Visibility = ViewStates.Gone;
                };
            }
        }

        private ValueAnimator BuildAnimator(int start, int end, View view)
        {
            var animator = ValueAnimator.OfInt(start, end);
            animator.Update += (object sender, ValueAnimator.AnimatorUpdateEventArgs e) =>
            {
                var value = (int)animator.AnimatedValue;
                ViewGroup.LayoutParams layoutParams = view.LayoutParameters;
                layoutParams.Height = value;
                view.LayoutParameters = layoutParams;
            };
            return animator;
        }
    }
}