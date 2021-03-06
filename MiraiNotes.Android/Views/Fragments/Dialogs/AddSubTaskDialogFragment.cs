﻿using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.ViewModels.Dialogs;
using MvvmCross.Platforms.Android.Presenters.Attributes;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    [MvxDialogFragmentPresentation(ActivityHostViewModelType = typeof(MainViewModel), Tag = nameof(AddSubTaskDialogFragment), Cancelable = true)]
    public class AddSubTaskDialogFragment : BaseDialogFragment<AddSubTaskDialogViewModel>
    {
        public override int LayoutId
            => Resource.Layout.AddSubTaskDialog;
    }
}