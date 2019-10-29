using MiraiNotes.Android.ViewModels;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class TaskListDeletedMsg : MvxMessage
    {
        public TaskListItemViewModel TaskList { get; set; }

        public TaskListDeletedMsg(object sender, TaskListItemViewModel taskList)
            : base(sender)
        {
            TaskList = taskList;
        }
    }
}