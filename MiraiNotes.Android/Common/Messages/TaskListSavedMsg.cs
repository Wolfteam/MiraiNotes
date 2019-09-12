using MiraiNotes.Android.ViewModels;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class TaskListSavedMsg : MvxMessage
    {
        public TaskListItemViewModel TaskList { get; }
        public TaskListSavedMsg(object sender, TaskListItemViewModel taskList) : base(sender)
        {
            TaskList = taskList;
        }
    }
}