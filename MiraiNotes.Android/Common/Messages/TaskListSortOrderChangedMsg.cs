using MiraiNotes.Core.Enums;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class TaskListSortOrderChangedMsg : MvxMessage
    {
        public TaskListSortType NewSortOrder { get; }

        public TaskListSortOrderChangedMsg(object sender, TaskListSortType newSortOrder) : base(sender)
        {
            NewSortOrder = newSortOrder;
        }
    }
}