using MiraiNotes.Data.Models;
using MiraiNotes.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiraiNotes.DataService.Interfaces
{
    public interface ITaskDataService : IRepository<GoogleTask>
    {
        /// <summary>
        /// Adds a task to the task list indicated by <paramref name="taskListID"/>
        /// </summary>
        /// <param name="taskListID">Id of the task list</param>
        /// <param name="task">The task to save</param>
        /// <returns><see cref="Response"/></returns>
        Task<Response<GoogleTask>> AddAsync(string taskListID, GoogleTask task);

        /// <summary>
        /// Adds multiple tasks to the specied task list in the db
        /// </summary>
        /// <param name="taskListID">Id of the task list</param>
        /// <param name="tasks">Task to save</param>
        /// <returns><see cref="Response"/></returns>
        Task<Response<IEnumerable<GoogleTask>>> AddRangeAsync(string taskListID, IEnumerable<GoogleTask> tasks);

        /// <summary>
        /// Moves a task to the selected task list
        /// </summary>
        /// <param name="selectedTaskListID">The selected task list where the task will be moved to</param>
        /// <param name="taskID">The id of the task to move</param>
        /// <param name="parentTask">Parent task identifier. If the task is created at the top level, this parameter is omitted. Optional.</param>
        /// <param name="previous">Previous sibling task identifier. If the task is created at the first position among its siblings, this parameter is omitted. Optional.</param>
        /// <returns><see cref="EmptyResponse"/></returns>
        Task<Response<GoogleTask>> MoveAsync(string selectedTaskListID, string taskID, string parentTask, string previous);

        /// <summary>
        /// Removes a task and the associated subtasks (if they exist).
        /// If the LocalStatus equals Created the task will be removed from db,
        /// otherwise the new LocalStatus will be Removed
        /// </summary>
        /// <param name="taskID">The id of the task to delete</param>
        /// <returns><see cref="EmptyResponse"/></returns>
        Task<EmptyResponse> RemoveTaskAsync(string taskID);

        /// <summary>
        /// Removes multiples task and their associated subtasks (if they exist).
        /// If the LocalStatus equals Created the task will be removed from db,
        /// otherwise the new LocalStatus will be Removed
        /// </summary>
        /// <param name="taskID">The ids of the tasks to delete</param>
        /// <returns><see cref="EmptyResponse"/></returns>
        Task<EmptyResponse> RemoveTaskAsync(IEnumerable<string> taskIds);
    }
}
