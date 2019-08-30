using MvvmCross.ViewModels;
using System;

namespace MiraiNotes.Android.ViewModels
{
    public class TaskListItemViewModel : MvxViewModel
    {
        private string _id;
        private string _title;
        private DateTimeOffset? _updatedAt;
        private int _numberOfTasks;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
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
    }
}