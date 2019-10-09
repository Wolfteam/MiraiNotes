using System;

namespace MiraiNotes.Abstractions.Services
{
    public interface ITelemetryService
    {
        void Init();
        void TrackError(Exception ex);
        void TrackEvent();
    }
}
