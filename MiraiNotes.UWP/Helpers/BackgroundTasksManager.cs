using Microsoft.Toolkit.Uwp.Helpers;
using MiraiNotes.UWP.BackgroundTasks;
using Windows.ApplicationModel.Background;

namespace MiraiNotes.UWP.Helpers
{
    public class BackgroundTasksManager
    {
        public static void RegisterBackgroundTasks()
        {
            if (BackgroundTaskHelper.IsBackgroundTaskRegistered(typeof(SyncBackgroundTask)))
            {
                // Background task already registered.
                return;
            }

            //TODO: CHANGE THE DEFAULT TIME THAT THE BGTask WILL GET EXECUTED
            BackgroundTaskHelper.Register(
                nameof(SyncBackgroundTask),
                new TimeTrigger(15, false),
                false,
                true,
                new SystemCondition(SystemConditionType.FreeNetworkAvailable),
                new SystemCondition(SystemConditionType.InternetAvailable));
        }

        public static void UnregisterBackgroundTasks()
        {
            BackgroundTaskHelper.Unregister(nameof(SyncBackgroundTask));
        }
    }
}
