namespace MiraiNotes.Android.Models.Results
{
    public class DeleteTaskDialogViewModelResult
    {
        public bool IsDeleted { get; }
        public bool IsPartiallyDeleted { get; }

        private DeleteTaskDialogViewModelResult(bool deleted, bool partiallyDeleted)
        {
            IsDeleted = deleted;
            IsPartiallyDeleted = partiallyDeleted;
        }

        public static DeleteTaskDialogViewModelResult Deleted(bool deleted)
            => new DeleteTaskDialogViewModelResult(deleted, false);

        public static DeleteTaskDialogViewModelResult Partial()
            => new DeleteTaskDialogViewModelResult(false, true);

        public static DeleteTaskDialogViewModelResult Nothing()
            => new DeleteTaskDialogViewModelResult(false, false);
    }
}