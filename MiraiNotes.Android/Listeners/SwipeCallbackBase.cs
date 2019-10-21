using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraiNotes.Android.Listeners
{
    public abstract class SwipeCallbackBase : ItemTouchHelper.SimpleCallback
    {
        public readonly Context Context;
        private readonly RecyclerView _recyclerView;
        private readonly Dictionary<int, List<SwipeButton>> _buttonsBuffer = new Dictionary<int, List<SwipeButton>>();
        private readonly Queue<int> _recoverQueue = new Queue<int>();
        private readonly object _lock = new object();
        private int _swipedPos = -1;
        private float _swipeThreshold = 0.5f;
        private GestureDetector _gestureDetector;

        public List<SwipeButton> Buttons { get; private set; } = new List<SwipeButton>();
        public virtual int ButtonWidth { get; set; } = (int)AndroidUtils.ToPixel(60, Application.Context);

        public SwipeCallbackBase(Context context, RecyclerView recyclerView)
            : base(0, ItemTouchHelper.Left | ItemTouchHelper.Right)
        {
            Context = context;
            _recyclerView = recyclerView;
            _gestureDetector = new GestureDetector(context, new SimpleOnGestureListener(this));
            _recyclerView.SetOnTouchListener(new TouchListener((v, e) => OnTouch(e)));

            AttachSwipe();
        }

        private bool OnTouch(MotionEvent e)
        {
            if (_swipedPos < 0)
                return false;
            var point = new Point((int)e.RawX, (int)e.RawY);
            RecyclerView.ViewHolder swipedViewHolder = _recyclerView.FindViewHolderForAdapterPosition(_swipedPos);
            View swipedItem = swipedViewHolder.ItemView;
            Rect rect = new Rect();
            swipedItem.GetGlobalVisibleRect(rect);

            if (e.Action == MotionEventActions.Down || e.Action == MotionEventActions.Up || e.Action == MotionEventActions.Move)
            {
                if (rect.Top < point.Y && rect.Bottom > point.Y)
                {
                    //System.Diagnostics.Debug.WriteLine("Click should happen");
                    try
                    {
                        _gestureDetector.OnTouchEvent(e);
                    }
                    catch (Exception)
                    {
                        //for some reason it crashes with a disposed ex..
                        SetGestureDetector();
                        _gestureDetector.OnTouchEvent(e);
                    }
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("No click..");
                    _recoverQueue.Enqueue(_swipedPos);
                    _swipedPos = -1;
                    RecoverSwipedItem();
                }
            }
            return false;
        }

        public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
        {
            return false;
        }

        public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
        {
            int pos = viewHolder.AdapterPosition;

            if (_swipedPos != pos)
                _recoverQueue.Enqueue(_swipedPos);

            _swipedPos = pos;

            if (_buttonsBuffer.ContainsKey(_swipedPos))
                Buttons = _buttonsBuffer[_swipedPos];
            else
                Buttons.Clear();

            _buttonsBuffer.Clear();
            _swipeThreshold = 0.5f * Buttons.Count * ButtonWidth;
            RecoverSwipedItem();
        }

        public override float GetSwipeThreshold(RecyclerView.ViewHolder viewHolder)
        {
            return _swipeThreshold;
        }

        public override float GetSwipeEscapeVelocity(float defaultValue)
        {
            return 0.1f * defaultValue;
        }

        public override float GetSwipeVelocityThreshold(float defaultValue)
        {
            return 5.0f * defaultValue;
        }

        public override void OnChildDraw(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState, bool isCurrentlyActive)
        {
            int pos = viewHolder.AdapterPosition;
            float translationX = dX;
            View itemView = viewHolder.ItemView;
            if (pos < 0)
            {
                _swipedPos = pos;
                return;
            }

            if (actionState == ItemTouchHelper.ActionStateSwipe)
            {
                List<SwipeButton> buffer = new List<SwipeButton>();
                if (dX < 0 || dX > 0)
                {
                    var position = dX < 0 ? UnderlayButtonPosition.Right : UnderlayButtonPosition.Left;
                    if (!_buttonsBuffer.ContainsKey(pos))
                    {
                        InstantiateUnderlayButton(viewHolder, buffer);
                        _buttonsBuffer.Add(pos, buffer);
                    }
                    else
                    {
                        buffer = _buttonsBuffer[pos];
                    }

                    translationX = dX * buffer.Count * ButtonWidth / itemView.Width;
                    DrawButtons(c, itemView, buffer, pos, translationX, position);
                }
            }

            base.OnChildDraw(c, recyclerView, viewHolder, translationX, dY, actionState, isCurrentlyActive);
        }

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

            // Tell the view holder it's time to restore the idle state
            if (viewHolder is IItemTouchHelperViewHolder itemViewHolder)
            {
                itemViewHolder.OnItemClear();
            }
        }

        public void ResetView()
        {
            _recoverQueue.Enqueue(_swipedPos);
            _swipedPos = -1;
            RecoverSwipedItem();
        }

        private void RecoverSwipedItem()
        {
            lock (_lock)
            {
                while (_recoverQueue.Any())
                {
                    int pos = _recoverQueue.Dequeue();
                    if (pos > -1)
                    {
                        _recyclerView.GetAdapter().NotifyItemChanged(pos);
                    }
                }
            }
        }

        private void DrawButtons(Canvas c, View itemView, List<SwipeButton> buffer, int pos, float dX, UnderlayButtonPosition position)
        {
            float right = itemView.Right;
            float left = itemView.Left;
            float dButtonWidth = (-1) * dX / buffer.Where(b => b.Position == position).Count();
            //System.Diagnostics.Debug.WriteLine($"Width = {itemView.Width} - MeasuredWdith = {itemView.MeasuredWidth}");
            foreach (var button in buffer)
            {
                if (dX < 0 && position == button.Position)
                {
                    left = right - dButtonWidth;
                    button.OnDraw(
                            c,
                            new RectF(
                                    left,
                                    itemView.Top,
                                    right,
                                    itemView.Bottom
                            ),
                            pos, dX, itemView
                    //(to draw button on right)
                    );
                    //System.Diagnostics.Debug.WriteLine($"Recf Right = {right} - Left = {left} - Top = {itemView.Top} - Bottom = {itemView.Bottom}");
                    right = left;
                }
                else if (dX > 0 && position == button.Position)
                {
                    right = left - dButtonWidth;
                    button.OnDraw(c,
                            new RectF(
                                    left,
                                    itemView.Top,
                                    right,
                                    itemView.Bottom
                            ), pos, dX, itemView
                    //(to draw button on left)

                    );
                    //System.Diagnostics.Debug.WriteLine($"Recf Right = {right} - Left = {left} - Top = {itemView.Top} - Bottom = {itemView.Bottom}");
                    left = right;
                }
            }
            //System.Diagnostics.Debug.WriteLine("Buttons were drawn");
        }

        private void AttachSwipe()
        {
            using (ItemTouchHelper itemTouchHelper = new ItemTouchHelper(this))
            {
                itemTouchHelper.AttachToRecyclerView(_recyclerView);
            }
        }

        private void SetGestureDetector()
        {
            _gestureDetector = new GestureDetector(Context, new SimpleOnGestureListener(this));
        }

        public abstract void InstantiateUnderlayButton(RecyclerView.ViewHolder viewHolder, List<SwipeButton> underlayButtons);
    }
}