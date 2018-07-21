using GalaSoft.MvvmLight;
using System;

namespace MiraiNotes.UWP.Models
{
    public class TaskListModel : ViewModelBase
    {
        private string _title;

        public string TaskListID { get; set; }

        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }
    }
}
