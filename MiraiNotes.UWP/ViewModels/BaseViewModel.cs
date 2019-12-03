using GalaSoft.MvvmLight;
using System;

namespace MiraiNotes.UWP.ViewModels
{
    public class BaseViewModel : ViewModelBase
    {
        /// <summary>
        /// Used to open a specific task inside a task list.
        /// Stores the taskListID and taskID provided from a launched or navigated args
        /// </summary>
        public static Tuple<int, int> InitDetails { get; set; }
    }
}
