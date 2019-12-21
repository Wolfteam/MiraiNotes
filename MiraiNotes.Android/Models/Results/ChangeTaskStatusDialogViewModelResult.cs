namespace MiraiNotes.Android.Models.Results
{
    public class ChangeTaskStatusDialogViewModelResult
    {
        public bool StatusChanged { get; }
        //This one is used for cases where you have multiple tasks and one change of them fails
        public bool PartiallyChanged { get; }

        public ChangeTaskStatusDialogViewModelResult(bool statusChanged, bool partiallyChanged)
        {
            StatusChanged = statusChanged;
            PartiallyChanged = partiallyChanged;
        }

        public static ChangeTaskStatusDialogViewModelResult Changed(bool changed)
            => new ChangeTaskStatusDialogViewModelResult(changed, false);

        public static ChangeTaskStatusDialogViewModelResult Partial()
            => new ChangeTaskStatusDialogViewModelResult(false, true);

        public static ChangeTaskStatusDialogViewModelResult Nothing()
            => new ChangeTaskStatusDialogViewModelResult(false, false);
    }
}