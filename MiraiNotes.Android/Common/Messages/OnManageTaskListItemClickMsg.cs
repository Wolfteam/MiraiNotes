using MiraiNotes.Android.ViewModels;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class OnManageTaskListItemClickMsg : MvxMessage
    {
        public bool Delete { get; }
        public bool Edit { get; }
        public TaskListItemViewModel TaskList { get; }

        public OnManageTaskListItemClickMsg(object sender, TaskListItemViewModel taskList, bool delete, bool edit)
            : base(sender)
        {
            Delete = delete;
            Edit = edit;
            TaskList = taskList;
        }
    }
}