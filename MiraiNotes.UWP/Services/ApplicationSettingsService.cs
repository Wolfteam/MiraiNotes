using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MiraiNotes.UWP.Services
{
    public class ApplicationSettingsService
    {
        //TODO: I SHOULD ADD A ENUM FOR THE VALUES
        public static int SyncBackgroundTaskInterval
        {
            get => (int)(ApplicationData.Current.LocalSettings.Values[nameof(SyncBackgroundTaskInterval)] ?? 0);
            set => ApplicationData.Current.LocalSettings.Values[nameof(SyncBackgroundTaskInterval)] = value;
        }
    }
}
