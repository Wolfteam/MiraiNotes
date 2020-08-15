using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using MiraiNotes.Android.Interfaces;
using MvvmCross;
using MvvmCross.Platforms.Android;
using System;

namespace MiraiNotes.Android.Listeners
{
    public class SimpleItemTouchHelperCallback : ItemTouchHelper.Callback
    {
        private readonly IItemTouchHelperAdapter _adapter;
        private readonly Button _rightButton;
        private readonly Button _leftButton;
        private readonly Drawable _rightToLefIcon;
        private readonly Drawable _leftToRightIcon;
        private readonly ColorDrawable _rightToLeftBackground;
        private readonly ColorDrawable _leftToRightBackground;
        private bool _swypeIsHappening;
        private readonly Context _context;
        private bool _reachedMaxSwypeWidth;
        public RecyclerView.ViewHolder CurrentViewHolder;

        private bool _swipeEnabled = true;
        public static float AlphaFull = 1.0f;
        private bool _swipeBack = false;
        private ButtonsState _buttonShowedState = ButtonsState.GONE;
        private static float _buttonWidth = 300;

        public SimpleItemTouchHelperCallback(IItemTouchHelperAdapter adapter)
        {
            _adapter = adapter;
            var currentTopActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            _context = currentTopActivity.Activity;

            _rightToLefIcon = ContextCompat.GetDrawable(_context, Resource.Drawable.ic_delete_black_24dp);
            _rightButton = new Button(_context)
            {
                Text = "Delete"
            };
            //_rightButton.SetBackgroundColor(Color.Transparent);
            _rightToLeftBackground = new ColorDrawable(Color.Red);

            _leftToRightIcon = ContextCompat.GetDrawable(_context, Resource.Drawable.ic_done_black_24dp);
            _leftButton = new Button(_context);
            _leftButton.Click += (sender, args) => System.Diagnostics.Debug.WriteLine("Click on the left button");
            _leftButton.SetBackgroundColor(Color.Green);
            _leftButton.Text = "Complete";
            _leftToRightBackground = new ColorDrawable(Color.ParseColor("#0582ff"));
        }

        public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
        {
            int dragFlags = 0;
            int swipeFlags = 0;
            // Set movement flags based on the layout manager
            if (recyclerView.GetLayoutManager() is GridLayoutManager)
            {
                dragFlags = ItemTouchHelper.Up | ItemTouchHelper.Down | ItemTouchHelper.Left | ItemTouchHelper.Right;
            }
            else
            {
                if (_swipeEnabled)
                {
                    swipeFlags = ItemTouchHelper.Left | ItemTouchHelper.Right;
                }
                else
                {
                    dragFlags = ItemTouchHelper.Up | ItemTouchHelper.Down;
                    swipeFlags = ItemTouchHelper.Start | ItemTouchHelper.End;
                }
            }
            //System.Diagnostics.Debug.WriteLine("-----------GetMovementFlags was called");

            return MakeMovementFlags(dragFlags, swipeFlags);
        }

        public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
        {
            if (viewHolder.ItemViewType != target.ItemViewType)
            {
                return false;
            }

            // Notify the adapter of the move
            return _adapter.OnItemMove(viewHolder.AdapterPosition, target.AdapterPosition);
        }

        public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
        {
            bool swipeToTheRight = direction == ItemTouchHelper.Right;
            // Notify the adapter of the dismissal
            _adapter.OnItemSwiped(viewHolder.AdapterPosition, swipeToTheRight);
        }

        public override void OnChildDraw(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState, bool isCurrentlyActive)
        {
            if (actionState == ItemTouchHelper.ActionStateSwipe)
            {
                // Fade out the view as it is swiped out of the parent's bounds
                //float alpha = AlphaFull - Math.Abs(dX) / (float)viewHolder.ItemView.Width;
                //viewHolder.ItemView.Alpha = alpha;
                //viewHolder.ItemView.TranslationX = dX;

                //SetTouchListener(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            }
            else
            {
            }

            //if (CurrentViewHolder != null && CurrentViewHolder.GetHashCode() != viewHolder.GetHashCode())
            //{
            //    ClearView(recyclerView, CurrentViewHolder);
            //    CurrentViewHolder = null;
            //}
            CurrentViewHolder = viewHolder;


            View itemView = viewHolder.ItemView;
            int backgroundCornerOffset = 20; //so background is behind the rounded corners of itemView
            //int iconHeight = _leftToRightIcon.IntrinsicHeight;
            //int iconWidth = _leftToRightIcon.IntrinsicWidth;

            int iconHeight = Dp2px(36);
            int iconWidth = Dp2px(36);
            //235px
            int iconMargin = (itemView.Height - iconHeight) / 2;
            int iconTop = itemView.Top + (itemView.Height - iconHeight) / 2;
            int iconBottom = iconTop + iconHeight;

            float newDx = dX;
            bool swypingToTheRight = dX > 0;
            int maxWidth = (int)(Math.Abs(viewHolder.ItemView.Width * 0.7) * (swypingToTheRight ? 1 : -1));
            //System.Diagnostics.Debug.WriteLine("Dx = {0} y maxWidth = {1}", dX, maxWidth);

            //System.Diagnostics.Debug.WriteLine("ItemLeft = {0}, ItemTop = {1}, ItemRight = {2}, ItemBottom = {3}", itemView.Left, itemView.Top, itemView.Right, itemView.Bottom);
            //DrawButtons(c, viewHolder);
            if (swypingToTheRight)
            {
                _swypeIsHappening = true;
                int iconLeft = itemView.Left + iconMargin;
                int iconRight = iconLeft + iconWidth;

                //_leftButton.Top = itemView.Top;
                //_leftButton.Bottom = itemView.Bottom;
                //_leftButton.Left =  iconLeft;
                //_leftButton.Right = iconRight;



                //System.Diagnostics.Debug.WriteLine("Swyping to the right. Icon Left = {0}, icon right = {1}, iconTop = {2}, iconBottom = {3}", iconLeft, iconRight, iconTop, iconBottom);

                _leftToRightBackground.SetBounds(
                    itemView.Left,
                    itemView.Top,
                    itemView.Left + ((int)dX) + backgroundCornerOffset,
                    itemView.Bottom);

                _leftToRightBackground.Draw(c);
                //_leftToRightIcon.Draw(c);

                for (int i = 0; i < 1; i++)
                {
                    iconLeft = (int)(itemView.Left + (i + 1) * iconMargin);
                    iconRight = iconLeft + iconWidth;
                    _leftToRightIcon.SetBounds(iconLeft, iconTop, iconRight, iconBottom);
                    _leftToRightIcon.Draw(c);
                }

                //_leftToRightIcon.Draw(c);
                //_leftButton.Draw(c);
            }
            else if (dX < 0) // Swiping to the left
            {
                _swypeIsHappening = true;
                int iconLeft = itemView.Right - iconMargin - iconWidth;
                int iconRight = itemView.Right - iconMargin;

                //System.Diagnostics.Debug.WriteLine("Swyping to the left. Icon Left = {0}, icon right = {1}, iconTop = {2}, iconBottom = {3}", iconLeft, iconRight, iconTop, iconBottom);

                _rightToLefIcon.SetBounds(iconLeft, iconTop, iconRight, iconBottom);
                _rightToLeftBackground.SetBounds(
                    itemView.Right + ((int)dX) - backgroundCornerOffset,
                    itemView.Top,
                    itemView.Right,
                    itemView.Bottom);

                _rightToLeftBackground.Draw(c);
                _rightToLefIcon.Draw(c);
            }
            else
            { // view is unSwiped

                _leftToRightBackground.SetBounds(0, 0, 0, 0);
                _rightToLeftBackground.SetBounds(0, 0, 0, 0);
                _swypeIsHappening = false;
            }

            //if (newDx != 0 && (swypingToTheRight && newDx >= maxWidth || !swypingToTheRight && newDx < maxWidth))
            //{
            //    _reachedMaxSwypeWidth = true;
            //    newDx = maxWidth;
            //    //itemView.TranslationX = maxWidth;
            //    return;
            //}
            //else
            //{
            //    _reachedMaxSwypeWidth = false;
            //}


            base.OnChildDraw(c, recyclerView, viewHolder, newDx, dY, actionState, isCurrentlyActive);

            //System.Diagnostics.Debug.WriteLine("-----------OnChildDraw was called");
        }

        //public override int ConvertToAbsoluteDirection(int flags, int layoutDirection)
        //{
        //    if (_swipeBack)
        //    {
        //        _swipeBack = _buttonShowedState != ButtonsState.GONE;
        //        return 0;
        //    }
        //    return base.ConvertToAbsoluteDirection(flags, layoutDirection);
        //}

        public override void OnSelectedChanged(RecyclerView.ViewHolder viewHolder, int actionState)
        {
            // We only want the active item to change
            if (actionState != ItemTouchHelper.ActionStateIdle)
            {
                // Let the view holder know that this item is being moved or dragged
                if (viewHolder is IItemTouchHelperViewHolder itemViewHolder)
                {
                    itemViewHolder.OnItemSelected();
                }
            }

            base.OnSelectedChanged(viewHolder, actionState);
        }

        public override void ClearView(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
        {
            base.ClearView(recyclerView, viewHolder);

            viewHolder.ItemView.Alpha = AlphaFull;

            // Tell the view holder it's time to restore the idle state
            if (viewHolder is IItemTouchHelperViewHolder itemViewHolder)
            {
                itemViewHolder.OnItemClear();
            }
            CurrentViewHolder = null;
        }

        public int Dp2px(int dp)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, _context.Resources.DisplayMetrics);
        }

        public void DrawButtons(Canvas c, RecyclerView.ViewHolder viewHolder)
        {
            var itemView = viewHolder.ItemView;
            int backgroundCornerOffset = 20; //so background is behind the rounded corners of itemView
            //int iconHeight = _leftToRightIcon.IntrinsicHeight;
            //int iconWidth = _leftToRightIcon.IntrinsicWidth;

            int iconHeight = Dp2px(36);
            int iconWidth = Dp2px(36);
            //235px
            int iconMargin = (itemView.Height - iconHeight) / 2;
            int iconTop = itemView.Top + (itemView.Height - iconHeight) / 2;
            int iconBottom = iconTop + iconHeight;

            int iconLeft = itemView.Left + iconMargin;
            int iconRight = iconLeft + iconWidth;

            //var swypeView = new LinearLayout(_context);
            //var paramss = new ViewGroup.LayoutParams(iconWidth, ViewGroup.LayoutParams.MatchParent);
            //var paramsP = new ViewGroup.LayoutParams(iconWidth - 2, ViewGroup.LayoutParams.MatchParent);
            //LinearLayout boss = new LinearLayout(_context)
            //{
            //    Orientation = Orientation.Horizontal,
            //    LayoutParameters = paramss
            //};
            //LinearLayout parent = new LinearLayout(_context)
            //{
            //    Orientation = Orientation.Vertical,
            //    LayoutParameters = paramsP,
            //    Background = _leftToRightBackground
            //};
            //parent.SetGravity(GravityFlags.Center);

            //swypeView.AddView(boss);
            //parent.AddView(CreateIcon(_leftToRightIcon));

            //parent.AddView(CreateTitle("Completed"));
            //View view = new View(_context)
            //{
            //    LayoutParameters = new ViewGroup.LayoutParams(2, ViewGroup.LayoutParams.MatchParent),
            //    Background = new ColorDrawable(Color.LightGray)
            //};
            //boss.AddView(view);
            //boss.AddView(parent);
            //swypeView.Draw(c);
            //_leftToRightBackground.SetBounds(itemView.Left, itemView.Top, itemView.Right + backgroundCornerOffset, itemView.Bottom);
            //_leftToRightIcon.SetBounds(iconLeft, iconTop, iconRight, iconBottom);
            //_leftButton.SetCompoundDrawables(null, _leftToRightIcon, null, null);
            //var frameLayout = new FrameLayout(_context)
            //{
            //    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent),
            //    Background = _leftToRightBackground,
            //    Top = itemView.Top,
            //    Bottom = itemView.Bottom,
            //    Right = itemView.Right + backgroundCornerOffset,
            //    Left = itemView.Left
            //};
            //var linearLayout = new LinearLayout(_context);
            //linearLayout.Orientation = Orientation.Horizontal;

            //if (_leftButton.Parent != null)
            //{
            //    ((ViewGroup)_leftButton.Parent).RemoveView(_leftButton);
            //}

            //linearLayout.AddView(_leftButton);
            //frameLayout.AddView(linearLayout);
            ////_leftToRightBackground.Draw(c);
            ////_leftButton.Draw(c);
            //frameLayout.Draw(c);
            //_leftToRightIcon.Draw(c);
        }

        //public void OnDraw(Canvas canvas)
        //{
        //    if (_currentViewHolder != null)
        //    {
        //        DrawButtons(canvas, _currentViewHolder);
        //    }
        //}



        private ImageView CreateIcon(Drawable icon)
        {
            ImageView iv = new ImageView(_context);
            iv.SetImageDrawable(icon);
            return iv;
        }

        private TextView CreateTitle(string text)
        {
            return new TextView(_context)
            {
                Text = text,
                Gravity = GravityFlags.Center,
                //TextSize = item.TitleSize,
                //tv.SetTextColor(item.TitleColor);
            };
        }









        private void SetTouchListener(Canvas c,
            RecyclerView recyclerView,
            RecyclerView.ViewHolder viewHolder,
            float dX, float dY,
            int actionState, bool isCurrentlyActive)
        {
            recyclerView.Touch -= (sender, args)
                => OnRecycleViewTouch(args, c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            recyclerView.Touch += (sender, args)
                => OnRecycleViewTouch(args, c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            //recyclerView.SetOnTouchListener(new TouchListener((v, me) =>
            //{
            //    _swipeBack =
            //        me.Action == MotionEventActions.Cancel ||
            //        me.Action == MotionEventActions.Up;

            //    if (_swipeBack)
            //    {
            //        if (dX < -_buttonWidth)
            //        {
            //            _buttonShowedState = ButtonsState.RIGHT_VISIBLE;
            //        }
            //        else if (dX > _buttonWidth)
            //        {
            //            _buttonShowedState = ButtonsState.LEFT_VISIBLE;
            //        }

            //        if (_buttonShowedState != ButtonsState.GONE)
            //        {
            //            SetTouchDownListener(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            //            SetItemsClickable(recyclerView, false);
            //        }
            //    }
            //    return false;
            //}));
        }


        private void OnRecycleViewTouch(
            View.TouchEventArgs args,
            Canvas c,
            RecyclerView recyclerView,
            RecyclerView.ViewHolder viewHolder,
            float dX, float dY,
            int actionState, bool isCurrentlyActive)
        {
            _swipeBack =
                args.Event.Action == MotionEventActions.Cancel ||
                args.Event.Action == MotionEventActions.Up;

            if (_swipeBack)
            {
                if (dX < -_buttonWidth)
                {
                    _buttonShowedState = ButtonsState.RIGHT_VISIBLE;
                }
                else if (dX > _buttonWidth)
                {
                    _buttonShowedState = ButtonsState.LEFT_VISIBLE;
                }

                if (_buttonShowedState != ButtonsState.GONE)
                {
                    SetTouchDownListener(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
                    SetItemsClickable(recyclerView, false);
                }
            }
            args.Handled = false;
        }

        private void SetTouchDownListener(
            Canvas c,
            RecyclerView recyclerView,
            RecyclerView.ViewHolder viewHolder,
            float dX, float dY,
            int actionState, bool isCurrentlyActive)
        {
            recyclerView.Touch -= (sender, args)
                => OnRecyclerViewTouchDown(args, c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            recyclerView.Touch += (sender, args)
                => OnRecyclerViewTouchDown(args, c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            //recyclerView.SetOnTouchListener(new TouchListener((v, me) =>
            //{
            //    if (me.Action == MotionEventActions.Down)
            //    {
            //        SetTouchUpListener(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            //    }
            //    return false;
            //}));
        }

        private void OnRecyclerViewTouchDown(
            View.TouchEventArgs args,
            Canvas c,
            RecyclerView recyclerView,
            RecyclerView.ViewHolder viewHolder,
            float dX, float dY,
            int actionState, bool isCurrentlyActive)
        {
            if (args.Event.Action == MotionEventActions.Down)
            {
                SetTouchUpListener(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            }
            args.Handled = false;
        }

        private void SetTouchUpListener(Canvas c,
             RecyclerView recyclerView,
             RecyclerView.ViewHolder viewHolder,
             float dX, float dY,
             int actionState, bool isCurrentlyActive)
        {
            recyclerView.Touch -= (sender, args)
                => OnRecyclerViewTouchUp(args, c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            recyclerView.Touch += (sender, args)
                => OnRecyclerViewTouchUp(args, c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
        }

        private void OnRecyclerViewTouchUp(
            View.TouchEventArgs args,
            Canvas c,
            RecyclerView recyclerView,
            RecyclerView.ViewHolder viewHolder,
            float dX, float dY,
            int actionState, bool isCurrentlyActive)
        {
            if (args.Event.Action == MotionEventActions.Up)
            {
                base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
                recyclerView.SetOnTouchListener(new TouchListener((view, motionEvent) => false));
                SetItemsClickable(recyclerView, true);
                _swipeBack = false;
                _buttonShowedState = ButtonsState.GONE;
            }
            args.Handled = false;
        }

        private void SetItemsClickable(RecyclerView recyclerView, bool isClickable)
        {
            for (int i = 0; i < recyclerView.ChildCount; ++i)
            {
                recyclerView.GetChildAt(i).Clickable = isClickable;
            }
        }

        enum ButtonsState
        {
            GONE,
            LEFT_VISIBLE,
            RIGHT_VISIBLE
        }
    }
}