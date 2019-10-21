using Android.Content;
using Android.Support.V7.Widget;
using System.Collections.Generic;

namespace MiraiNotes.Android.Listeners
{
    public class SwipeCallback : SwipeCallbackBase
    {
        public List<SwipeButton> AddedButons { get; }

        public SwipeCallback(Context context, RecyclerView recyclerView, List<SwipeButton> buttons) : base(context, recyclerView)
        {
            AddedButons = buttons;
        }

        public override void InstantiateUnderlayButton(RecyclerView.ViewHolder viewHolder, List<SwipeButton> underlayButtons)
        {
            underlayButtons.AddRange(AddedButons);
        }
    }
}