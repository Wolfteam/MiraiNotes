using MvvmCross.ViewModels;
using System;

namespace MiraiNotes.Android.ViewModels
{
    public class TaskListItemViewModel : MvxViewModel
    {
        private string _googleId;
        private string _title;
        private DateTimeOffset? _updatedAt;
        private int _numberOfTasks;
        private bool _isSelected;

        public int Id { get; set; }

        public string GoogleId
        {
            get => _googleId;
            set => SetProperty(ref _googleId, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public DateTimeOffset? UpdatedAt
        {
            get => _updatedAt;
            set => SetProperty(ref _updatedAt, value);
        }

        //you need to manually set this one
        public int NumberOfTasks
        {
            get => _numberOfTasks;
            set => SetProperty(ref _numberOfTasks, value);
        }

        //you need to manually set this one
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}