namespace MiraiNotes.Android.Models.Results
{
    public class MoveTaskDialogViewModelResult
    {
        public bool WasMoved { get; }
        public bool PartiallyMoved { get; }

        private MoveTaskDialogViewModelResult(bool moved, bool partiallyMoved)
        {
            WasMoved = moved;
            PartiallyMoved = partiallyMoved;
        }

        public static MoveTaskDialogViewModelResult Moved(bool moved)
            => new MoveTaskDialogViewModelResult(moved, false);

        public static MoveTaskDialogViewModelResult Partial()
            => new MoveTaskDialogViewModelResult(false, true);

        public static MoveTaskDialogViewModelResult Nothing()
            => new MoveTaskDialogViewModelResult(false, false);
    }
}