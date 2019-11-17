using MiraiNotes.Android.ViewModels;
using MiraiNotes.Core.Enums;
using System;

namespace MiraiNotes.Android.Models.Parameters
{
    public class TaskDateViewModelParameter
    {
        public bool SendUpdateMsg { get; }
        public TaskListItemViewModel TaskList { get; }
        public TaskItemViewModel Task { get; }
        public TaskNotificationDateType DateType { get; }

        private TaskDateViewModelParameter(
            TaskListItemViewModel taskList, 
            TaskItemViewModel task, 
            TaskNotificationDateType dateType,
            bool sendUpdatedMsg)
        {
            TaskList = taskList ?? throw new ArgumentNullException(nameof(taskList));
            Task = task ?? throw new ArgumentNullException(nameof(task));
            DateType = dateType;
            SendUpdateMsg = sendUpdatedMsg;
        }

        public static TaskDateViewModelParameter Instance(
            TaskListItemViewModel taskList, 
            TaskItemViewModel task, 
            TaskNotificationDateType dateType,
            bool sendUpdatedMsg = false)
        {
            return new TaskDateViewModelParameter(taskList, task, dateType, sendUpdatedMsg);
        }
    }
}