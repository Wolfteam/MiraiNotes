﻿using GalaSoft.MvvmLight;
using System;

namespace MiraiNotes.UWP.ViewModels
{
    public class TaskListItemViewModel : ViewModelBase
    {
        private string _title;

        public int Id { get; set; }

        public string TaskListID { get; set; }

        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
