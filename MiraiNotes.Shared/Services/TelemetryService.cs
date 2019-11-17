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
            if (AppCenter.Configured)
                return;
            AppCenter.Start(Secrets.AppCenterSecret, typeof(Analytics), typeof(Crashes));
        }

        public void TrackError(Exception ex)
        {
            Crashes.TrackError(ex);
        }

        public void TrackEvent()
        {
            throw new NotImplementedException();
        }
    }
}
