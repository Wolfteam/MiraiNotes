using MiraiNotes.Shared.Models;
using System;

namespace MiraiNotes.Shared.Helpers
{
    public static class GoogleTaskStatusHelper
    {
        private const string NEEDS_ACTION_STATUS = "needsAction";
        private const string COMPLETED_STATUS = "completed";

        /// <summary>
        /// Gets the google status string associated to <paramref name="status"/>
        /// </summary>
        /// <param name="status"></param>
        /// <returns>Returns the google status string</returns>
        public static string GetString(this GoogleTaskStatus status)
        {
            switch (status)
            {
                case GoogleTaskStatus.NEEDS_ACTION:
                    return NEEDS_ACTION_STATUS;
                case GoogleTaskStatus.COMPLETED:
                    return COMPLETED_STATUS;
                default:
                    throw new ArgumentNullException($"The provided status {status} doesnt exists");
            }
        }

        public static GoogleTaskStatus GetGoogleStatus(string status)
        {
            switch (status)
            {
                case NEEDS_ACTION_STATUS:
                    return GoogleTaskStatus.NEEDS_ACTION;
                case COMPLETED_STATUS:
                    return GoogleTaskStatus.COMPLETED;
                default:
                    throw new ArgumentNullException($"The provided status {status} doesnt exists");
            }
        }
    }
}
