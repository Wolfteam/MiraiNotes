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
        /// <returns><see cref="Result"/></returns>
        Task<Result> AddAsync(string taskListID, GoogleTask task);

        /// <summary>
        /// Adds multiple tasks to the specied task list in the db
        /// </summary>
        /// <param name="taskListID">Id of the task list</param>
        /// <param name="tasks">Task to save</param>
        /// <returns><see cref="Result"/></returns>
        Task<Result> AddRangeAsync(string taskListID, IEnumerable<GoogleTask> tasks);

        /// <summary>
        /// Moves a task to the selected task list
        /// </summary>
        /// <param name="selectedTaskListID">The selected task list where the task will be moved to</param>
        /// <param name="taskID">The id of the task to move</param>
        /// <param name="position">The position</param>
        /// <returns><see cref="Result"/></returns>
        //Task<Result> MoveAsync(string selectedTaskListID, string taskID, string position);
    }
}
