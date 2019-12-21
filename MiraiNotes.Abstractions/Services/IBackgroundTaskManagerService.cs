using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;

namespace MiraiNotes.Abstractions.Services
{
    public interface IBackgroundTaskManagerService
    {
        /// <summary>
        /// Registers a particular bg tasks specified by <paramref name="backgroundTask"/>
        /// </summary>
        /// <param name="backgroundTask">The background task to register</param>
        /// <param name="restart">Indicates if the registration of the bg task is mandatory(True by default)</param>
        void RegisterBackgroundTasks(BackgroundTaskType backgroundTask, bool restart = true);

        /// <summary>
        /// Unregister a background task specified by <paramref name="backgroundTask"/>
        /// </summary>
        /// <param name="backgroundTask">The background task to unregister</param>
        void UnregisterBackgroundTasks(BackgroundTaskType backgroundTask = BackgroundTaskType.ANY);

        /// <summary>
        /// Manually starts a specific backgroun task
        /// </summary>
        /// <param name="backgroundTask">The background task to start</param>
        void StartBackgroundTask(BackgroundTaskType backgroundTask);

        /// <summary>
        /// Manually starts a specific backgroun task
        /// </summary>
        /// <param name="backgroundTask">The background task to start</param>
        /// <param name="parameter">A parameter that will be used by the <paramref name="backgroundTask"/></param>
        void StartBackgroundTask(BackgroundTaskType backgroundTask, BackgroundTaskParameter parameter);
    }
}
