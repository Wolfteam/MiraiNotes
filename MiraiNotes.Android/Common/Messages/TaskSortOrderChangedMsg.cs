using MiraiNotes.Core.Enums;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class TaskSortOrderChangedMsg : MvxMessage
    {
        public TaskSortType NewSortOrder { get; }

        public TaskSortOrderChangedMsg(object sender, TaskSortType newSortOrder) : base(sender)
        {
            NewSortOrder = newSortOrder;
        }
    }
}