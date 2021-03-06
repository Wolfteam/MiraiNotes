﻿using MiraiNotes.Android.ViewModels;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class TaskListSavedMsg : MvxMessage
    {
        public bool WasUpdated { get;}
        public TaskListItemViewModel TaskList { get; }
        public TaskListSavedMsg(object sender, TaskListItemViewModel taskList, bool wasUpdated) : base(sender)
        {
            TaskList = taskList;
            WasUpdated = wasUpdated;
        }
    }
}