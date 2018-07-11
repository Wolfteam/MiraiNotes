using GalaSoft.MvvmLight;
using MiraiNotes.UWP.ViewModels;
using System;

namespace MiraiNotes.UWP.Models
{
    public class TaskModel : BaseViewModel

    {
        public string TaskID { get; set; }

        public string Title { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string SelfLink { get; set; }

        public string ParentTask { get; set; }

        public string Position { get; set; }

        public string Notes { get; set; }

        public string Status { get; set; }

        public DateTime? ToBeCompletedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsHidden { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetValue(ref isSelected, value); }
        }
    }
}
