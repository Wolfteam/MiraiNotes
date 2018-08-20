using System;

namespace MiraiNotes.UWP.Interfaces
{
    public interface IDispatcherHelper
    {
        void CheckBeginInvokeOnUi(Action action);
    }
}
