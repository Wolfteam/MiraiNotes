using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MiraiNotes.Abstractions.Services;
using System;

namespace MiraiNotes.Shared.Services
{
    public class TelemetryService : ITelemetryService
    {
        public void Init()
        {
#if !DEBUG
            if (AppCenter.Configured)
                return;
            AppCenter.Start(Secrets.AppCenterSecret, typeof(Analytics), typeof(Crashes));
#endif
        }

        public void TrackError(Exception ex)
        {
#if !DEBUG
            Crashes.TrackError(ex);
#endif
        }

        public void TrackEvent()
        {
            throw new NotImplementedException();
        }
    }
}
