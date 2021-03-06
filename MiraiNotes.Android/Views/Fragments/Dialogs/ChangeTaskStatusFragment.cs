﻿using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.ViewModels.Dialogs;
using MvvmCross.Platforms.Android.Presenters.Attributes;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    [MvxDialogFragmentPresentation(ActivityHostViewModelType = typeof(MainViewModel), Tag = nameof(ChangeTaskStatusFragment), Cancelable = true)]
    public class ChangeTaskStatusFragment : BaseDialogFragment<ChangeTaskStatusDialogViewModel>
    {
        public override int LayoutId 
            => Resource.Layout.ConfirmationDialog;
    }
}