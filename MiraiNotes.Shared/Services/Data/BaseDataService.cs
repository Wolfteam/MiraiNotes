using MiraiNotes.Abstractions.Services;
using System;

namespace MiraiNotes.Shared.Services.Data
{
    public class BaseDataService
    {
        private readonly ITelemetryService _telemetryService;

        public BaseDataService(ITelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
        }

        protected string GetExceptionMessage(Exception e)
        {
            string inner = e.InnerException?.Message;
            string result;
            if (!string.IsNullOrEmpty(inner))
                result = $"{e.Message}. Inner Exception: {inner}";
            else
                result = $"{e.Message}. StackTrace: {e.StackTrace}";

            _telemetryService.TrackError(e);
            return result;
        }
    }
}
