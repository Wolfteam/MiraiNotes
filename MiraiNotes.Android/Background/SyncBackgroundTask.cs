using Android.Content;
using AndroidX.Work;
using MiraiNotes.Android.Common.Utils;
using MvvmCross.Platforms.Android.Core;

namespace MiraiNotes.Android.Background
{
    public class SyncBackgroundTask : Worker
    {
        private readonly Context _context;
        public SyncBackgroundTask(Context context, WorkerParameters workerParams)
            : base(context, workerParams)
        {
            _context = context;
        }

        public override Result DoWork()
        {
            var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(ApplicationContext);
            setup.EnsureInitialized();
            _context.StartForegroundServiceCompat<SyncBackgroundService>();

            return Result.InvokeSuccess();
        }
    }
}