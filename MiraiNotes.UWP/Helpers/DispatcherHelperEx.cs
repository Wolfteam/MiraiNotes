using GalaSoft.MvvmLight.Threading;
using MiraiNotes.UWP.Interfaces;
using System;

namespace MiraiNotes.UWP.Helpers
{
    public class DispatcherHelperEx : IDispatcherHelper
    {
        public void CheckBeginInvokeOnUi(Action action)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(action);
        }
    }
}
