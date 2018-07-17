using MiraiNotes.UWP.Models;
using System;

namespace MiraiNotes.UWP.Helpers
{
    public static class GoogleTaskStatusHelper
    {
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
                    return "needsAction";
                case GoogleTaskStatus.COMPLETED:
                    return "completed";
                default:
                    throw new ArgumentNullException($"The provided status {status} doesnt exists");
            }
        }
    }
}
