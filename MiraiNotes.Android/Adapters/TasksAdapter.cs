using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.ViewModels;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.ViewModels;
using System.Linq;

namespace MiraiNotes.Android.Adapters
{
    public class TasksAdapter : MvxRecyclerAdapter, IItemTouchHelperAdapter
    {
        public TasksAdapter(IMvxAndroidBindingContext bindingContext)
            : base(bindingContext)
        {
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, BindingContext.LayoutInflaterHolder);
            var view = InflateViewForHolder(parent, viewType, itemBindingContext);

            return new TaskViewHolder(view, itemBindingContext);
            //var view = BindingContext.BindingInflate(Resource.Layout.TaskItem, parent);
            //var itemViewHolder = new TaskViewHolder(view, BindingContext);
            //return itemViewHolder;
        }

        //NOT USED
        public void OnItemSwiped(int position, bool swipedToTheRight)
        {
            var items = (MvxObservableCollection<TaskItemViewModel>)ItemsSource;
            var item = items.ElementAt(position);

            if (!swipedToTheRight)
            {
                //item.DeleteTaskCommand.Execute();
                //items.Remove(item);
                //NotifyDataSetChanged();
                //NotifyItemRemoved(position);
                //NotifyItemRangeChanged(position, ItemCount - position);
            }
            else
            {
                //item.ChangeTaskStatusCommand.Execute();
                items.Remove(item);
                items.Add(item);
            }
        }

        //NOT USED
        public bool OnItemMove(int fromPosition, int toPosition)
        {
            var items = (MvxObservableCollection<TaskItemViewModel>)ItemsSource;
            items.Move(fromPosition, toPosition);
            NotifyItemMoved(fromPosition, toPosition);
            return true;
        }

        public class TaskViewHolder : MvxRecyclerViewHolder, IItemTouchHelperViewHolder
        {
            private readonly View _itemView;
            private readonly Drawable _initialColor;

            public TaskViewHolder(View itemView, IMvxAndroidBindingContext context)
                : base(itemView, context)
            {
                _itemView = itemView;
                _initialColor = _itemView.Background;
            }

            public void OnItemSelected()
            {
                _itemView.SetBackgroundColor(Color.LightGray);
            }

            public void OnItemClear()
            {
                ViewCompat.SetBackground(_itemView, _initialColor);
            }
        }
    }
}