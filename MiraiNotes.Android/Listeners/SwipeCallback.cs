using Android.Content;
using Android.Support.V7.Widget;
using System.Collections.Generic;

namespace MiraiNotes.Android.Listeners
{
    public class SwipeCallback : SwipeCallbackBase
    {
        private readonly Context context;
        public List<SwipeButton> AddedButons { get; }

        public SwipeCallback(Context context, RecyclerView recyclerView, List<SwipeButton> buttons) : base(context, recyclerView)
        {
            this.context = context;
            AddedButons = buttons;
        }

        public override void InstantiateUnderlayButton(RecyclerView.ViewHolder viewHolder, List<SwipeButton> underlayButtons)
        {
            var buttons = new List<SwipeButton>();
            foreach (var button in AddedButons)
            {
                buttons.Add(new SwipeButton(
                    context, button.Id, button.Text,
                    button.ImageResId, button.BackgroundColor, button.IconColor,
                    button.TextColor, button.TextSize,
                    button.Position, button.Listener));
            }

            underlayButtons.AddRange(buttons);
        }
    }
}