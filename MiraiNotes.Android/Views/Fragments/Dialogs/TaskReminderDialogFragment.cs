using Android.App;
using Android.OS;
using Android.Text.Format;
using Android.Views;
using Android.Widget;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.ViewModels.Dialogs;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using System;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    [MvxDialogFragmentPresentation(ActivityHostViewModelType = typeof(MainViewModel), Tag = nameof(TaskReminderDialogFragment), Cancelable = true)]
    public class TaskReminderDialogFragment : BaseDialogFragment<TaskReminderDialogViewModel>, 
        DatePickerDialog.IOnDateSetListener, 
        TimePickerDialog.IOnTimeSetListener
    {
        public override int LayoutId
            => Resource.Layout.TaskReminderDialog;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);

            var dateButton = view.FindViewById<Button>(Resource.Id.TaskReminderDate);
            var timeButton = view.FindViewById<Button>(Resource.Id.TaskReminderTime);

            dateButton.Click += (sender, args) =>
            {
                var currentDate = DateTime.Now;
                var dialog = new DatePickerDialog(Activity, this, currentDate.Year, currentDate.Month - 1, currentDate.Day);
                dialog.DatePicker.SetMinDate(ViewModel.MinDate);
                dialog.Show();
            };

            timeButton.Click += (sender, args) =>
            {
                var currentTime = DateTime.Now;
                bool is24HourFormat = DateFormat.Is24HourFormat(Activity);
                var dialog = new TimePickerDialog(Activity, Resource.Style.TimePickerDialogCustom, this, currentTime.Hour, currentTime.Minute, is24HourFormat);
                dialog.Show();
            };

            return view;
        }

        public void OnDateSet(DatePicker view, int year, int month, int dayOfMonth)
        {
            // Note: monthOfYear is a value between 0 and 11, not 1 and 12!
            var selectedDate = new DateTime(year, month + 1, dayOfMonth);
            ViewModel.SetReminderDateText(selectedDate);
        }

        public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
        {
            var currentTime = DateTime.Now;
            var selectedTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, hourOfDay, minute, 0);
            ViewModel.SetReminderHourTexxt(selectedTime);
        }
    }
}