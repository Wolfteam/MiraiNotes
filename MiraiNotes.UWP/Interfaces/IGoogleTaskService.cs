using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Models.API;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Interfaces
{
    public interface IGoogleTaskService
    {
        /// <summary>
        /// Gets all tasks in the specified <paramref name="taskListID"/>
        /// </summary>
        /// <param name="taskListID">The task list id</param>
        /// <param name="maxResults">Maximum number of task lists returned on one page. Optional. The default is 100.</param>
        /// <param name="pageToken">Token specifying the result page to return. Optional.</param>
        /// <returns>Returns a google response model with the task lists</returns>
        Task<GoogleResponseModel<GoogleTaskApiResponseModel<GoogleTaskModel>>> GetAllAsync(
            string taskListID, 
            int maxResults = 100, 
            string pageToken = null);

        Task<GoogleResponseModel<GoogleTaskModel>> GetAsync(string taskListID, string taskID);

        /// <summary>
        /// Saves a task into the task list specfied by: <paramref name="taskListID"/>
        /// </summary>
        /// <param name="taskListID">The task list id</param>
        /// <param name="task">The task object to save</param>
        /// <param name="parent">New parent task identifier. If the task is moved to the top level, this parameter is omitted. Optional.</param>
        /// <param name="previous">New previous sibling task identifier. If the task is moved to the first position among its siblings, this parameter is omitted. Optional.</param>
        /// <returns>GoogleResponseModel of type GoogleTaskModel </returns>
        Task<GoogleResponseModel<GoogleTaskModel>> SaveAsync(
            string taskListID, 
            GoogleTaskModel task, 
            string parent = null, 
            string previous = null);

        /// <summary>
        /// Updates a task in the specified task list
        /// </summary>
        /// <param name="taskListID">The task list id</param>
        /// <param name="taskID">The task id</param>
        /// <param name="task">The task object</param>
        /// <returns>GoogleResponseModel of type GoogleTaskModel</returns>
        Task<GoogleResponseModel<GoogleTaskModel>> UpdateAsync(string taskListID, string taskID, GoogleTaskModel task);

        /// <summary>
        /// Removes a task from the specified task list
        /// </summary>
        /// <param name="taskListID">The task list id</param>
        /// <param name="taskID">The task id</param>
        /// <returns>GoogleEmptyResponseModel</returns>
        Task<GoogleEmptyResponseModel> DeleteAsync(string taskListID, string taskID);

        /// <summary>
        /// Clears all completed tasks from the specified task list. 
        /// The affected tasks will be marked as 'hidden' and no longer be returned by default when retrieving all tasks for a task list
        /// </summary>
        /// <param name="taskListID">The task list id</param>
        /// <returns>GoogleEmptyResponseModel</returns>
        Task<GoogleEmptyResponseModel> ClearAsync(string taskListID);

        /// <summary>
        /// Moves the specified task to another position in the task list. 
        /// This can include putting it as a child task under a new parent and/or move it to a different position among its sibling tasks
        /// </summary>
        /// <param name="taskListID">The task list id</param>
        /// <param name="taskID">The task id</param>
        /// <param name="parent">New parent task identifier. If the task is moved to the top level, this parameter is omitted. Optional.</param>
        /// <param name="previous">New previous sibling task identifier. If the task is moved to the first position among its siblings, this parameter is omitted. Optional.</param>
        /// <returns>GoogleResponseModel of type GoogleTaskModel</returns>
        Task<GoogleResponseModel<GoogleTaskModel>> MoveAsync(
            string taskListID, 
            string taskID, 
            string parent = null, 
            string previous = null);

        /// <summary>
        /// Moves the <paramref name="task"/> 
        /// from <paramref name="currentTaskListID"/> to the <paramref name="selectedTaskListID"/>
        /// </summary>
        /// <param name="task">A google task object with all their properties</param>
        /// <param name="currentTaskListID">The current task list id where this task is located</param>
        /// <param name="selectedTaskListID">The selected task list id were this task will be stored</param>
        /// <param name="parent">New parent task identifier. If the task is moved to the top level, this parameter is omitted. Optional.</param>
        /// <param name="previous">New previous sibling task identifier. If the task is moved to the first position among its siblings, this parameter is omitted. Optional.</param>
        /// <returns>GoogleResponseModel of type GoogleTaskModel</returns>
        Task<GoogleResponseModel<GoogleTaskModel>> MoveAsync(
            GoogleTaskModel task, 
            string currentTaskListID, 
            string selectedTaskListID, 
            string parent = null, 
            string previous = null);

        /// <summary>
        /// Changes the status of a task
        /// </summary>
        /// <param name="taskListID">The task list id</param>
        /// <param name="taskID">The task id</param>
        /// <param name="newTaskStatus">The status that will be applied to the task</param>
        /// <returns>GoogleResponseModel of type GoogleTaskModel</returns>
        Task<GoogleResponseModel<GoogleTaskModel>> ChangeStatus(
            string taskListID, 
            string taskID, 
            GoogleTaskStatus newTaskStatus);
    }
}
