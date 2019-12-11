using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;

namespace MiraiNotes.Abstractions.Data
{
    public interface ITaskDataService
    {
        Task<ResponseDto<GoogleTask>> FirstOrDefaultAsNoTrackingAsync(Expression<Func<GoogleTask, bool>> predicate);

        Task<ResponseDto<IEnumerable<GoogleTask>>> GetAsNoTrackingAsync(Expression<Func<GoogleTask, bool>> filter = null, Func<IQueryable<GoogleTask>, IOrderedQueryable<GoogleTask>> orderBy = null, string includeProperties = "");

        Task<ResponseDto<IEnumerable<GoogleTask>>> GetAsync(Expression<Func<GoogleTask, bool>> filter = null, Func<IQueryable<GoogleTask>, IOrderedQueryable<GoogleTask>> orderBy = null, string includeProperties = "");

        Task<EmptyResponseDto> RemoveAsync(GoogleTask entity);

        Task<EmptyResponseDto> RemoveAsync(Expression<Func<GoogleTask, bool>> filter);

        Task<EmptyResponseDto> RemoveRangeAsync(IEnumerable<GoogleTask> entities);

        Task<ResponseDto<GoogleTask>> UpdateAsync(GoogleTask entity);

        /// <summary>
        /// Adds a task to the task list indicated by <paramref name="taskListID"/>
        /// </summary>
        /// <param name="taskListID">Id of the task list</param>
        /// <param name="task">The task to save</param>
        /// <returns><see cref="Response"/></returns>
        Task<ResponseDto<GoogleTask>> AddAsync(string taskListID, GoogleTask task);

        /// <summary>
        /// Adds multiple tasks to the specied task list in the db
        /// </summary>
        /// <param name="taskListID">Id of the task list</param>
        /// <param name="tasks">Task to save</param>
        /// <returns><see cref="Response"/></returns>
        Task<ResponseDto<IEnumerable<GoogleTask>>> AddRangeAsync(string taskListID, IEnumerable<GoogleTask> tasks);

        /// <summary>
        /// Moves a task to the selected task list
        /// </summary>
        /// <param name="selectedTaskListID">The selected task list where the task will be moved to</param>
        /// <param name="taskID">The id of the task to move</param>
        /// <param name="parentTask">Parent task identifier. If the task is created at the top level, this parameter is omitted. Optional.</param>
        /// <param name="previous">Previous sibling task identifier. If the task is created at the first position among its siblings, this parameter is omitted. Optional.</param>
        /// <returns><see cref="EmptyResponseDto"/></returns>
        Task<ResponseDto<GoogleTask>> MoveAsync(string selectedTaskListID, string taskID, string parentTask, string previous);

        /// <summary>
        /// Removes a task and the associated subtasks (if they exist).
        /// If the LocalStatus equals Created the task will be removed from db,
        /// otherwise the new LocalStatus will be Removed
        /// </summary>
        /// <param name="taskID">The id of the task to delete</param>
        /// <returns><see cref="EmptyResponseDto"/></returns>
        Task<EmptyResponseDto> RemoveTaskAsync(string taskID);

        /// <summary>
        /// Removes multiples task and their associated subtasks (if they exist).
        /// If the LocalStatus equals Created the task will be removed from db,
        /// otherwise the new LocalStatus will be Removed
        /// </summary>
        /// <param name="taskID">The ids of the tasks to delete</param>
        /// <returns><see cref="EmptyResponseDto"/></returns>
        Task<EmptyResponseDto> RemoveTaskAsync(IEnumerable<string> taskIds);

        /// <summary>
        /// Changes the task status to the one indicated by <paramref name="taskStatus"/>
        /// </summary>
        /// <param name="taskID">The ids of the tasks to delete</param>
        /// <param name="taskStatus">The new task status</param>
        /// <returns><see cref="ResponseDto{GoogleTask}"/></returns>
        Task<ResponseDto<GoogleTask>> ChangeTaskStatusAsync(string taskID, GoogleTaskStatus taskStatus);

        /// <summary>
        /// Removes the notiifcation date of the specified task
        /// </summary>
        /// <param name="taskID">The task id</param>
        /// <param name="dateType">The type of date to remove</param>
        /// <returns><see cref="ResponseDto{GoogleTask}"/></returns>
        Task<ResponseDto<GoogleTask>> RemoveNotificationDate(string taskID, TaskNotificationDateType dateType);

        /// <summary>
        /// Adds a notification date to the specified task
        /// </summary>
        /// <param name="taskID">The task id</param>
        /// <param name="dateType">The type of date to remove</param>
        /// <param name="remindOn">The date</param>
        /// <param name="remindOnGuid">An integer string (only used when <paramref name="dateType"/> equals <see cref="TaskNotificationDateType.REMINDER_DATE"/>)</param>
        /// <returns><see cref="ResponseDto{GoogleTask}"/></returns>
        Task<ResponseDto<GoogleTask>> AddNotificationDate(string taskID, TaskNotificationDateType dateType, DateTimeOffset remindOn, string remindOnGuid);

        /// <summary>
        /// Gets the lastest position in the specified <paramref name="taskListId"/>
        /// </summary>
        /// <param name="taskListId">The google tasklist id</param>
        /// <param name="parentTask">The parent task id. If you supply this param, 
        /// you will get the lastest position inside a task (AKA the lastest subtask position inside this task)
        /// </param>
        /// <returns>The lastest position</returns>
        Task<ResponseDto<string>> GetLastestPosition(string taskListId, string parentTask = null);

        /// <summary>
        /// Gets the lastest taskId in the specified <paramref name="taskListId"/>
        /// </summary>
        /// <param name="taskListId">The google tasklist id</param>
        /// <param name="parentTask">The parent task id. If you supply this param, 
        /// you will get the lastest taskId inside a task (AKA the lastest subtaskid inside this task)
        /// </param>
        /// <returns>The lastest taskId</returns>
        Task<ResponseDto<string>> GetPreviousTaskId(string taskListId, string parentTask);
    }
}
