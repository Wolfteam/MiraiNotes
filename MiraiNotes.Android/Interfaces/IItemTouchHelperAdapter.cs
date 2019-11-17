namespace MiraiNotes.Android.Interfaces
{
    /// <summary>
    /// Interface to listen for a move or dismissal event from a ItemTouchHelper.Callback.
    /// </summary>
    public interface IItemTouchHelperAdapter
    {
        /// <summary>
        /// Called when an item has been dragged far enough to trigger a move. This is called every time
        /// an item is shifted, and <strong>not</strong> at the end of a "drop" event.<br/>
        /// <br/>
        /// Implementations should call {@link RecyclerView.Adapter#notifyItemMoved(int, int)} after
        /// adjusting the underlying data to reflect this move.
        /// </summary>
        /// <returns>True if the item was moved to the new adapter position.</returns>
        /// <param name="fromPosition">The start position of the moved item.</param>
        /// <param name="toPosition">Then resolved position of the moved item.</param>
        bool OnItemMove(int fromPosition, int toPosition);

        void OnItemSwiped(int position, bool swipedToTheRight);
    }
}