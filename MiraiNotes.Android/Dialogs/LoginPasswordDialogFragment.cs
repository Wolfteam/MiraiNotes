using System;
using Android.App;
using Android.OS;
using Android.Widget;
using MvvmCross;
using MvvmCross.Platforms.Android;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace MiraiNotes.Android.Dialogs
{
    public class LoginPasswordDialogFragment : DialogFragment
    {
        private readonly Action<string> _onOkClick;
        private readonly Action _onCancelClick;

        public LoginPasswordDialogFragment(Action<string> onOkClick, Action onCancelClick)
        {
            _onOkClick = onOkClick;
            _onCancelClick = onCancelClick;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            var top = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();

            return new AlertDialog.Builder(top.Activity)
                .SetView(Resource.Layout.LoginPasswordDialog)
                .SetMessage("Type your password")
                .SetPositiveButton("OK", (sender, args) =>
                {
                    var txt = GetTypedText();
                    if (string.IsNullOrWhiteSpace(txt))
                    {
                        var editText = top.Activity.FindViewById<EditText>(Resource.Id.LoginPasswordEditText);
                        editText.RequestFocus();
                        return;
                    }

                    _onOkClick(txt);
                })
                .SetNegativeButton("Cancel", (sender, args) => _onCancelClick())
                .Create();
        }


        private string GetTypedText()
        {
            var top = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            var txt = top.Activity.FindViewById<EditText>(Resource.Id.LoginPasswordEditText);
            return txt.Text;
        }
    }
}