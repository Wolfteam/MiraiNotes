using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android
{
    public class CultureChangedMessage : MvxMessage
    {
        public CultureChangedMessage(object sender) : base(sender)
        {
        }
    }
}