using Android.Content;
using AndroidX.Work;

namespace MiraiNotes.Android.Background
{
    public class SyncBackgroundTask : Worker
    {
        public SyncBackgroundTask(Context context, WorkerParameters workerParams)
            : base(context, workerParams)
        {
        }

        public override Result DoWork()
        {
            new SyncTask(false, null).Sync().GetAwaiter().GetResult();
            return Result.InvokeSuccess();
        }
    }
}