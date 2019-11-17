namespace MiraiNotes.Android.Interfaces
{
    /// <summary>
    /// Interface to notify an item ViewHolder of relevant callbacks from
    /// android.support.v7.widget.helper.ItemTouchHelper.Callback.
    /// </summary>
    public interface IItemTouchHelperViewHolder
    {
        /// <summary>
        /// Called when the ItemTouchHelper first registers an item as being moved or swiped.
        /// Implementations should update the item view to indicate it's active state.
        /// </summary>
        void OnItemSelected();

        /// <summary>
        /// Called when the ItemTouchHelper has completed the move or swipe, and the active item
        /// state should be cleared.
        /// </summary>
        void OnItemClear();
    }
}