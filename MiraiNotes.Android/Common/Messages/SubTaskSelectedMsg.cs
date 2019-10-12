using MiraiNotes.Android.ViewModels;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class SubTaskSelectedMsg : MvxMessage
    {
        public TaskItemViewModel SubTask { get; }
        public bool ShowMenuOptions { get; }

        public SubTaskSelectedMsg(object sender, TaskItemViewModel subTask, bool showMenuOptions = false) : base(sender)
        {
            SubTask = subTask;
            ShowMenuOptions = showMenuOptions;
        }
    }
}