using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models.GoogleApi;
using Serilog;

//TODO: ONCE UWP IS USING REFIT, MOVE THIS TO THE SHARED PROJECT
namespace MiraiNotes.Android.Services
{
    public class SyncService : ISyncService
    {
        private readonly IGoogleApiService _apiService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly INetworkService _networkService;
        private readonly ILogger _logger;

        public SyncService(
            IGoogleApiService apiService, 
            IMiraiNotesDataService dataService, 
            INetworkService networkService, 
            ILogger logger)
        {
            _apiService = apiService;
            _dataService = dataService;
            _networkService = networkService;
            _logger = logger.ForContext<SyncService>();
        }
        
        #region Public sync methods


        public async Task<EmptyResponseDto> SyncDownTaskListsAsync(bool isInBackground)
        {
            _logger.Information($"{nameof(SyncDownTaskListsAsync)}: Starting the sync down of task lists");
            var syncResult = new EmptyResponseDto
            {
                Message = string.Empty,
                Succeed = false
            };

            var syncDownResults = new List<EmptyResponseDto>();

            if (!_networkService.IsInternetAvailable())
            {
                syncResult.Message = "Network is not available";
                _logger.Warning($"{nameof(SyncDownTaskListsAsync)}: Network is not available");
                return syncResult;
            }

            string nextPageToken = null;
            bool hasMorePages = true;
            while (hasMorePages)
            {
                var response = await _apiService.GetAllTaskLists(pageToken: nextPageToken);

                nextPageToken = response.Result?.NextPageToken;
                if (string.IsNullOrEmpty(nextPageToken))
                    hasMorePages = false;

                if (!response.Succeed)
                {
                    //TODO: I SHOULD DO SOMETHING HERE...
                    syncResult.Message = response.Message ?? "Unkwnon error";
                    _logger.Error(
                        $"{nameof(SyncDownTaskListsAsync)}: Couldn't get all the task lists from api. " +
                        $"Error = {syncResult.Message}");
                    return syncResult;
                }

                var downloadedTaskLists = response.Result.Items.ToList();

                var dbResponse = await _dataService
                    .TaskListService
                    .GetAsNoTrackingAsync(tl => tl.User.IsActive);

                if (!dbResponse.Succeed)
                {
                    _logger.Error(
                        $"{nameof(SyncDownTaskListsAsync)}: Couldn't get all the task lists from db. " +
                        $"Error = {dbResponse.Message}");
                    return dbResponse;
                }

                var tasks = new List<Task>
                {
                    //Here we save any new remote task list
                    Task.Run(async () =>
                    {
                        var taskListsToSave = downloadedTaskLists
                            .Where(dt => dbResponse.Result.All(ct => ct.GoogleTaskListID != dt.TaskListID))
                            .Select(t => new GoogleTaskList
                            {
                                GoogleTaskListID = t.TaskListID,
                                CreatedAt = DateTimeOffset.UtcNow,
                                Title = t.Title,
                                UpdatedAt = t.UpdatedAt
                            }).ToList();
                        if (!taskListsToSave.Any())
                            return;
                        _logger.Information(
                            $"{nameof(SyncDownTaskListsAsync)}: Trying to save into db {taskListsToSave.Count} " +
                            $"new remote task lists");

                        var r = await _dataService
                            .TaskListService
                            .AddRangeAsync(taskListsToSave);
                        if (r.Succeed)
                            syncDownResults.Add(r);
                    }),

                    //Here we delete any task list that is not in remote
                    Task.Run(async () =>
                    {
                        var deletedTaskLists = dbResponse.Result
                            .Where(ct =>
                                downloadedTaskLists.All(dt => dt.TaskListID != ct.GoogleTaskListID) &&
                                ct.LocalStatus != LocalStatus.CREATED)
                            .ToList();

                        if (!deletedTaskLists.Any())
                            return;
                        _logger.Information(
                            $"{nameof(SyncDownTaskListsAsync)}: Trying to delete from db {deletedTaskLists.Count} task lists");

                        var r = await _dataService
                            .TaskListService
                            .RemoveRangeAsync(deletedTaskLists);

                        if (r.Succeed)
                            syncDownResults.Add(r);
                    }),

                    //Here we update the local tasklists
                    Task.Run(async () =>
                    {
                        foreach (var taskList in dbResponse.Result)
                        {
                            var t = downloadedTaskLists
                                .FirstOrDefault(dt => dt.TaskListID == taskList.GoogleTaskListID);

                            if (t == null)
                                return;

                            if (taskList.UpdatedAt >= t.UpdatedAt)
                                continue;
                            _logger.Information(
                                $"{nameof(SyncDownTaskListsAsync)}: Trying to update the local tasklistId = {taskList.ID}");
                            taskList.Title = t.Title;
                            taskList.UpdatedAt = t.UpdatedAt;
                            var r = await _dataService
                                .TaskListService
                                .UpdateAsync(taskList);

                            if (r.Succeed)
                                syncDownResults.Add(r);
                        }
                    })
                };

                await Task.WhenAll(tasks);
            }

            if (syncDownResults.Any(r => !r.Succeed))
                syncResult.Message = string.Join($".{Environment.NewLine}", syncDownResults.Select(r => r.Message));
            else
                syncResult.Succeed = true;
            _logger.Information($"{nameof(SyncDownTaskListsAsync)}: Completed successfully");
            return syncResult;
        }

        public async Task<EmptyResponseDto> SyncDownTasksAsync(bool isInBackground)
        {
            _logger.Information($"{nameof(SyncDownTasksAsync)}: Starting the sync down of tasks");
            var syncResult = new EmptyResponseDto
            {
                Message = string.Empty,
                Succeed = false
            };

            var syncDownResults = new List<EmptyResponseDto>();

            if (!_networkService.IsInternetAvailable())
            {
                syncResult.Message = "Network is not available";
                _logger.Warning($"{nameof(SyncDownTasksAsync)}: Network is not available");
                return syncResult;
            }

            var dbResponse = await _dataService
                .TaskListService
                .GetAsNoTrackingAsync(
                    tl => tl.User.IsActive &&
                          tl.LocalStatus != LocalStatus.CREATED &&
                          tl.LocalStatus != LocalStatus.DELETED);

            if (!dbResponse.Succeed)
            {
                _logger.Error(
                    $"{nameof(SyncDownTasksAsync)}: Couldn't get all the task lists from db. " +
                    $"Error = {dbResponse.Message}");
                return dbResponse;
            }
            foreach (var taskList in dbResponse.Result)
            {
                _logger.Information(
                    $"{nameof(SyncDownTasksAsync)}: Trying to get all the tasks " +
                    $"associated to taskListId = {taskList.ID} from api");
                string nextPageToken = null;
                bool hasMorePages = true;
                while (hasMorePages)
                {
                    var response = await _apiService.GetAllTasks(taskList.GoogleTaskListID, pageToken: nextPageToken);

                    nextPageToken = response.Result?.NextPageToken;
                    if (string.IsNullOrEmpty(nextPageToken))
                        hasMorePages = false;

                    if (!response.Succeed)
                    {
                        //TODO: I SHOULD DO SOMETHING HERE TOO...
                        syncResult.Message = response.Message ?? "Unknown error";
                        syncResult.Succeed = false;
                        _logger.Error(
                            $"{nameof(SyncDownTasksAsync)}: Couldn't get all the tasks associated to " +
                            $"taskListId = {taskList.ID} from api. Error = {syncResult.Message}");
                        return syncResult;
                    }

                    var downloadedTasks = response.Result?.Items?.ToList();

                    //if this task list doesnt contains task
                    if (downloadedTasks == null || !downloadedTasks.Any())
                    {
                        _logger.Information(
                            $"{nameof(SyncDownTasksAsync)}: Task list = {taskList.Title} does not contains tasks, " +
                            $"trying to remove any local task associated to it");
                        var deleteResponse = await _dataService
                            .TaskService
                            .RemoveAsync(
                                t => t.TaskList.GoogleTaskListID == taskList.GoogleTaskListID &&
                                     t.LocalStatus != LocalStatus.CREATED);
                        syncDownResults.Add(deleteResponse);
                        continue;
                    }

                    _logger.Information(
                        $"{nameof(SyncDownTasksAsync)}: Trying to get all the tasks associated to " +
                        $"tasklistID = {taskList.GoogleTaskListID} from db");
                    //I think i dont need to include the tasklist property
                    var currentTasksDbResponse = await _dataService
                        .TaskService
                        .GetAsync(t => t.TaskList.GoogleTaskListID == taskList.GoogleTaskListID, null, string.Empty);

                    if (!currentTasksDbResponse.Succeed)
                    {
                        _logger.Error(
                            $"{nameof(SyncDownTasksAsync)}: Couldn't get all the tasks associated to " +
                            $"tasklistID = {taskList.GoogleTaskListID} from db");
                        return currentTasksDbResponse;
                    }

                    var tasks = new List<Task>
                    {
                        //Here we save any new remote task
                        Task.Run(async () =>
                        {
                            var tasksToSave = downloadedTasks
                                .Where(dt => currentTasksDbResponse.Result.All(ct => ct.GoogleTaskID != dt.TaskID))
                                .Select(t => new GoogleTask
                                {
                                    GoogleTaskID = t.TaskID,
                                    CreatedAt = DateTimeOffset.UtcNow,
                                    Title = t.Title,
                                    UpdatedAt = t.UpdatedAt,
                                    CompletedOn = t.CompletedOn,
                                    IsDeleted = t.IsDeleted,
                                    IsHidden = t.IsHidden,
                                    Notes = t.Notes,
                                    ParentTask = t.ParentTask,
                                    Position = t.Position,
                                    Status = t.Status,
                                    ToBeCompletedOn = t.ToBeCompletedOn
                                }).ToList();

                            if (!tasksToSave.Any())
                                return;
                            _logger.Information(
                                $"{nameof(SyncDownTasksAsync)}: Trying to save into db {tasksToSave.Count} " +
                                $"new remote task associated to tasklistID = {taskList.GoogleTaskListID}");

                            syncDownResults.Add(await _dataService
                                .TaskService
                                .AddRangeAsync(taskList.GoogleTaskListID, tasksToSave));

                        }),

                        //Here we delete any task that is not in remote
                        Task.Run(async () =>
                        {
                            var deletedTasks = currentTasksDbResponse.Result
                                .Where(ct =>
                                    downloadedTasks.All(dt => dt.TaskID != ct.GoogleTaskID) &&
                                    ct.LocalStatus != LocalStatus.CREATED)
                                .ToList();

                            if (!deletedTasks.Any())
                                return;
                            _logger.Information(
                                $"{nameof(SyncDownTasksAsync)}: Trying to delete from db {deletedTasks.Count} " +
                                $"tasks associated to tasklistID = {taskList.GoogleTaskListID}");

                            syncDownResults.Add(await _dataService
                                .TaskService
                                .RemoveRangeAsync(deletedTasks));
                        }),

                        //Here we update the local tasks
                        Task.Run(async () =>
                        {
                            foreach (var task in currentTasksDbResponse.Result)
                            {
                                var downloadedTask = downloadedTasks
                                    .FirstOrDefault(dt => dt.TaskID == task.GoogleTaskID);

                                if (downloadedTask == null)
                                    return;

                                if (task.UpdatedAt >= downloadedTask.UpdatedAt)
                                    continue;
                                _logger.Information(
                                    $"{nameof(SyncDownTasksAsync)}: Trying to update the local taskId = {task.ID} " +
                                    $"associated to tasklistID = {taskList.ID}");
                                task.CompletedOn = downloadedTask.CompletedOn;
                                task.GoogleTaskID = downloadedTask.TaskID;
                                task.IsDeleted = downloadedTask.IsDeleted;
                                task.IsHidden = downloadedTask.IsHidden;
                                task.Notes = downloadedTask.Notes;
                                task.ParentTask = downloadedTask.ParentTask;
                                task.Position = downloadedTask.Position;
                                task.Status = downloadedTask.Status;
                                task.Title = downloadedTask.Title;
                                task.ToBeCompletedOn = downloadedTask.ToBeCompletedOn;
                                task.UpdatedAt = downloadedTask.UpdatedAt;

                                syncDownResults.Add(await _dataService
                                    .TaskService
                                    .UpdateAsync(task));
                            }
                        })
                    };

                    await Task.WhenAll(tasks);
                }
            }

            if (syncDownResults.Any(r => !r.Succeed))
                syncResult.Message = string.Join($".{Environment.NewLine}", syncDownResults.Select(r => r.Message));
            else
                syncResult.Succeed = true;
            _logger.Information($"{nameof(SyncDownTasksAsync)}: Completed successfully");
            return syncResult;
        }

        public async Task<EmptyResponseDto> SyncUpTaskListsAsync(bool isInBackground)
        {
            _logger.Information($"{nameof(SyncUpTaskListsAsync)}: Starting the sync up of task lists");
            var syncUpResult = new EmptyResponseDto
            {
                Succeed = false,
                Message = string.Empty
            };
            var syncUpResults = new List<EmptyResponseDto>();

            if (!_networkService.IsInternetAvailable())
            {
                syncUpResult.Message = "Network is not available";
                _logger.Warning($"{nameof(SyncUpTaskListsAsync)}: Network is not available");
                return syncUpResult;
            }

            var taskListToSyncDbResponse = await _dataService
                .TaskListService
                .GetAsNoTrackingAsync(
                    taskList => taskList.ToBeSynced && taskList.User.IsActive,
                    taskList => taskList.OrderBy(tl => tl.UpdatedAt),
                    string.Empty);

            if (!taskListToSyncDbResponse.Succeed)
            {
                _logger.Error(
                    $"{nameof(SyncUpTaskListsAsync)}: Couldn't get all the task lists to sync from db. " +
                    $"Error = {taskListToSyncDbResponse.Message}");
                return taskListToSyncDbResponse;
            }

            var tasks = new List<Task>
            {
                //Here we take the taskLists that were created
                Task.Run(async () =>
                {
                    var createdTaskLists = taskListToSyncDbResponse.Result
                        .Where(tl => tl.LocalStatus == LocalStatus.CREATED)
                        .ToList();

                    if (!createdTaskLists.Any())
                        return;
                    _logger.Information(
                        $"{nameof(SyncUpTaskListsAsync)}: Trying to save remotely {createdTaskLists.Count} task lists");

                    foreach (var taskList in createdTaskLists)
                        syncUpResults.Add(await SaveUpTaskList(taskList));
                }),

                //Here we take the tasklists that were deleted
                Task.Run(async () =>
                {
                    var deletedTaskLists = taskListToSyncDbResponse.Result
                        .Where(tl => tl.LocalStatus == LocalStatus.DELETED)
                        .ToList();

                    if (!deletedTaskLists.Any())
                        return;
                    _logger.Information(
                        $"{nameof(SyncUpTaskListsAsync)}: Trying to delete remotely {deletedTaskLists.Count} task lists");

                    foreach (var taskList in deletedTaskLists)
                        syncUpResults.Add(await DeleteUpTaskList(taskList));
                }),

                //Here we take the taskLists that were updated
                Task.Run(async () =>
                {
                    var updatedTaskLists = taskListToSyncDbResponse.Result
                        .Where(tl => tl.LocalStatus == LocalStatus.UPDATED)
                        .ToList();

                    if (!updatedTaskLists.Any())
                        return;
                    _logger.Information(
                        $"{nameof(SyncUpTaskListsAsync)}: Trying to delete remotely {updatedTaskLists.Count} task lists");

                    foreach (var taskList in updatedTaskLists)
                        syncUpResults.Add(await UpdateUpTaskList(taskList));
                })
            };

            await Task.WhenAll(tasks);

            if (syncUpResults.Any(r => !r.Succeed))
            {
                syncUpResult.Message = string.Join(
                    $".{Environment.NewLine}",
                    syncUpResults
                        .Where(r => !r.Succeed)
                        .Select(r => r.Message));
            }
            else
                syncUpResult.Succeed = true;

            _logger.Information($"{nameof(SyncUpTaskListsAsync)}: Completed successfully");
            return syncUpResult;
        }

        public async Task<EmptyResponseDto> SyncUpTasksAsync(bool isInBackground)
        {
            _logger.Information($"{nameof(SyncUpTasksAsync)}: Starting the sync up of tasks");
            var syncUpResult = new EmptyResponseDto
            {
                Message = string.Empty,
                Succeed = false
            };

            var syncUpResults = new List<EmptyResponseDto>();

            if (!_networkService.IsInternetAvailable())
            {
                syncUpResult.Message = "Network is not available";
                _logger.Warning($"{nameof(SyncUpTasksAsync)}: Network is not available");
                return syncUpResult;
            }

            var tasksToBeSyncedDbResponse = await _dataService
                .TaskService
                .GetAsNoTrackingAsync(
                    task => task.ToBeSynced && task.TaskList.User.IsActive,
                    includeProperties: nameof(GoogleTask.TaskList));

            if (!tasksToBeSyncedDbResponse.Succeed)
            {
                _logger.Error(
                    $"{nameof(SyncUpTasksAsync)}: Couldn't get all the tasks to sync from db. " +
                    $"Error = {tasksToBeSyncedDbResponse.Message}");
                return tasksToBeSyncedDbResponse;
            }

            var tasks = new List<Task>
            {
                //Here we save the tasks that were created locally
                Task.Run(async () =>
                {
                    var tasksToCreate = tasksToBeSyncedDbResponse.Result
                        .Where(t => t.LocalStatus == LocalStatus.CREATED)
                        .OrderBy(t => t.ParentTask).ThenBy(t => t.Position)
                        .ToList();

                    if (!tasksToCreate.Any())
                        return;
                    _logger.Information(
                        $"{nameof(SyncUpTasksAsync)}: Trying to save remotely {tasksToCreate.Count} tasks");

                    foreach (var task in tasksToCreate)
                    {
                        _logger.Information($"{nameof(SyncUpTasksAsync)}: Trying to save remotely taskId = {task.ID}");
                        string localID = task.GoogleTaskID;
                        var result = await SaveUpTask(task);
                        syncUpResults.Add(result);

                        if (!result.Succeed)
                            continue;

                        //If this isn't a sub task, lets update the sub tasks that belong to this one
                        if (string.IsNullOrEmpty(task.ParentTask))
                        {
                            _logger.Information(
                                $"{nameof(SyncUpTasksAsync)}: Checking if taskId = {task.ID} contains subtask, " +
                                $"and if it does, we are updating their parenttask with the new googletaskid");
                            foreach (var st in tasksToCreate
                                .Where(st => st.ParentTask == localID))
                            {
                                _logger.Information(
                                    $"{nameof(SyncUpTasksAsync)}: TaskId = {st.ID} is a subtask of taskId = {task.ID}");
                                st.ParentTask = task.GoogleTaskID;
                            }
                        }
                        //If this is a sub task and there are sts whose position depends in the current one
                        //we need to update them
                        else if (tasksToCreate.Any(st => st.Position == localID))
                        {
                            //In theory this should only affect 1 st
                            foreach (var st in tasksToCreate
                                .Where(st => st.Position == localID))
                            {
                                st.Position = task.GoogleTaskID;
                            }
                        }
                    }
                }),

                //Here we save the tasks that were deleted locally
                Task.Run(async () =>
                {
                    var tasksToDelete = tasksToBeSyncedDbResponse.Result
                        .Where(t => t.LocalStatus == LocalStatus.DELETED)
                        .OrderBy(t => t.ParentTask).ThenBy(t => t.Position)
                        .ToList();

                    if (!tasksToDelete.Any())
                        return;
                    _logger.Information(
                        $"{nameof(SyncUpTasksAsync)}: Trying to delete remotely {tasksToDelete.Count} tasks");

                    foreach (var task in tasksToDelete)
                    {
                        _logger.Information($"{nameof(SyncUpTasksAsync)}: Trying to delete remotely taskId = {task.ID}");
                        syncUpResults.Add(await DeleteUpTask(task));
                    }
                }),

                //Here we update the tasks that were updated locally
                Task.Run(async () =>
                {
                    var tasksToUpdate = tasksToBeSyncedDbResponse.Result
                        .Where(t => t.LocalStatus == LocalStatus.UPDATED)
                        .OrderBy(t => t.ParentTask).ThenBy(t => t.Position)
                        .ToList();

                    if (!tasksToUpdate.Any())
                        return;
                    _logger.Information(
                        $"{nameof(SyncUpTasksAsync)}: Trying to update remotely {tasksToUpdate.Count} tasks");

                    foreach (var task in tasksToUpdate)
                    {
                        _logger.Information($"{nameof(SyncUpTasksAsync)}: Trying to update remotely taskId = {task.ID}");
                        syncUpResults.Add(await UpdateUpTask(task));
                    }
                })
            };

            await Task.WhenAll(tasks);

            if (syncUpResults.Any(r => !r.Succeed))
            {
                syncUpResult.Message = string.Join(
                    $".{Environment.NewLine}",
                    syncUpResults
                        .Where(r => !r.Succeed)
                        .Select(r => r.Message));
            }
            else
                syncUpResult.Succeed = true;

            _logger.Information($"{nameof(SyncUpTasksAsync)}: Completed successfully");
            return syncUpResult;
        }
        #endregion

        #region Private task list sync methods
        private async Task<EmptyResponseDto> SaveUpTaskList(GoogleTaskList taskList)
        {
            var response = await _apiService.SaveTaskList(new GoogleTaskListModel
            {
                Title = taskList.Title,
                UpdatedAt = taskList.UpdatedAt
            });

            var result = new EmptyResponseDto
            {
                Succeed = response.Succeed
            };
            if (response.Succeed)
            {
                taskList.GoogleTaskListID = response.Result.TaskListID;
                taskList.ToBeSynced = false;
                taskList.LocalStatus = LocalStatus.DEFAULT;
                taskList.UpdatedAt = response.Result.UpdatedAt;
                result = await _dataService
                    .TaskListService
                    .UpdateAsync(taskList);
            }
            else
            {
                result.Message = response.Message ??
                                 $"An unkwon error occurred while trying to create task list {taskList.Title}";
                _logger.Error(
                    $"{nameof(SyncUpTaskListsAsync)}: An error occurred while trying to " +
                    $"save remotely taskListId = {taskList.ID}. {result.Message}");
            }
            return result;
        }

        private async Task<EmptyResponseDto> DeleteUpTaskList(GoogleTaskList taskList)
        {
            var response = await _apiService.DeleteTaskList(taskList.GoogleTaskListID);

            var result = new EmptyResponseDto
            {
                Succeed = response.Succeed
            };

            if (response.Succeed)
                result = await _dataService.TaskListService.RemoveAsync(taskList);
            else
            {
                result.Message = response.Message ??
                                 $"An unkwon error occurred while trying to delete task list {taskList.Title}";
                _logger.Error(
                    $"{nameof(SyncUpTaskListsAsync)}: An error occurred while trying to " +
                    $"delete remotely taskListId = {taskList.ID}. {result.Message}");
            }

            return result;
        }

        private async Task<EmptyResponseDto> UpdateUpTaskList(GoogleTaskList taskList)
        {
            var response = await _apiService.GetTaskList(taskList.GoogleTaskListID);

            var result = new EmptyResponseDto
            {
                Succeed = response.Succeed
            };
            if (!response.Succeed)
            {
                result.Message = response.Message ??
                                 $"An unknow error occurred while trying to get {taskList.Title} from remote to be updated";
                _logger.Error(
                    $"{nameof(SyncUpTaskListsAsync)}: An error occurred while trying to get the task to " +
                    $"update remotely taskListId = {taskList.ID}. {result.Message}");
            }
            //We need to update the remote contrapart
            else if (taskList.UpdatedAt > response.Result.UpdatedAt)
            {
                response.Result.UpdatedAt = taskList.UpdatedAt;
                response.Result.Title = taskList.Title;
                response = await _apiService.UpdateTaskList(response.Result.TaskListID, response.Result);

                result.Succeed = response.Succeed;

                if (!response.Succeed)
                {
                    result.Message = response.Message ??
                                     $"An unknow error occurred while trying to get {taskList.Title} from remote to be updated";
                    _logger.Error(
                        $"{nameof(SyncUpTaskListsAsync)}: An error occurred while trying to " +
                        $"update remotely taskListId = {taskList.ID}. {result.Message}");
                }
                else
                {
                    taskList.LocalStatus = LocalStatus.DEFAULT;
                    taskList.ToBeSynced = false;
                    result = await _dataService.TaskListService.UpdateAsync(taskList);
                }
            }
            //we need to update the local contrapart
            else
            {
                taskList.Title = response.Result.Title;
                taskList.LocalStatus = LocalStatus.DEFAULT;
                taskList.ToBeSynced = false;
                taskList.UpdatedAt = response.Result.UpdatedAt;
                result = await _dataService.TaskListService.UpdateAsync(taskList);
            }

            return result;
        }
        #endregion

        #region Private task sync methods
        private async Task<EmptyResponseDto> SaveUpTask(GoogleTask task)
        {
            var t = new GoogleTaskModel
            {
                Notes = task.Notes,
                Status = task.Status,
                Title = task.Title,
                ToBeCompletedOn = task.ToBeCompletedOn,
                UpdatedAt = task.UpdatedAt
            };

            var response = await _apiService.SaveTask(task.TaskList.GoogleTaskListID, t, task.ParentTask, task.Position);

            var result = new EmptyResponseDto
            {
                Succeed = response.Succeed
            };

            if (response.Succeed)
            {
                task.LocalStatus = LocalStatus.DEFAULT;
                task.ToBeSynced = false;
                task.GoogleTaskID = response.Result.TaskID;
                task.UpdatedAt = response.Result.UpdatedAt;
                task.Position = response.Result.Position;
                task.ParentTask = response.Result.ParentTask;

                result = await _dataService.TaskService.UpdateAsync(task);
            }
            else
            {
                result.Message = response.Message ??
                                 $"An unkwon error occurred while trying to create task {task.Title}";
                _logger.Error(
                    $"{nameof(SyncUpTasksAsync)}: An error occurred while trying to " +
                    $"save remotely taskId = {task.ID}. {result.Message}");
            }

            return result;
        }

        private async Task<EmptyResponseDto> DeleteUpTask(GoogleTask task)
        {
            var response = await _apiService.DeleteTask(task.TaskList.GoogleTaskListID, task.GoogleTaskID);

            var result = new EmptyResponseDto
            {
                Succeed = response.Succeed
            };

            if (response.Succeed)
                result = await _dataService.TaskService.RemoveAsync(task);
            else
            {
                result.Message = response.Message ??
                                 $"An unkwon error occurred while trying to delete task {task.Title}";
                _logger.Error(
                    $"{nameof(SyncUpTasksAsync)}: An error occurred while trying to " +
                    $"delete remotely taskId = {task.ID}. {result.Message}",
                    task, result.Message);
            }

            return result;
        }

        private async Task<EmptyResponseDto> UpdateUpTask(GoogleTask task)
        {
            var response = await _apiService.GetTask(task.TaskList.GoogleTaskListID, task.GoogleTaskID);

            var result = new EmptyResponseDto
            {
                Succeed = response.Succeed
            };
            if (!response.Succeed)
            {
                result.Message = response.Message ??
                                 $"An unkwon error occurred while trying to get task {task.Title}";
                _logger.Error(
                    $"{nameof(SyncUpTasksAsync)}: An error occurred while trying to get the task to " +
                    $"update remotely taskId = {task.ID}. {result.Message}");
            }
            else if (task.UpdatedAt > response.Result.UpdatedAt)
            {
                response.Result.CompletedOn = task.CompletedOn;
                response.Result.IsDeleted = task.IsDeleted;
                response.Result.Notes = task.Notes;
                response.Result.Status = task.Status;
                response.Result.Title = task.Title;
                response.Result.ToBeCompletedOn = task.ToBeCompletedOn;
                response.Result.UpdatedAt = task.UpdatedAt;

                response = await _apiService.UpdateTask(task.TaskList.GoogleTaskListID, task.GoogleTaskID, response.Result);

                result.Succeed = response.Succeed;

                if (response.Succeed)
                {
                    task.LocalStatus = LocalStatus.DEFAULT;
                    task.ToBeSynced = false;
                    result = await _dataService.TaskService.UpdateAsync(task);
                }
                else
                {
                    result.Message = response.Message ??
                                     $"An unkwon error occurred while trying to delete task {task.Title}";
                    _logger.Error(
                        $"{nameof(SyncUpTasksAsync)}: An error occurred while trying to " +
                        $"update remotely taskId = {task.ID}. {result.Message}");
                }
            }
            else
            {
                task.CompletedOn = response.Result.CompletedOn;
                task.IsDeleted = response.Result.IsDeleted;
                task.IsHidden = response.Result.IsHidden;
                task.Notes = response.Result.Notes;
                task.ParentTask = response.Result.ParentTask;
                task.Position = response.Result.Position;
                task.Status = response.Result.Status;
                task.Title = response.Result.Title;
                task.ToBeCompletedOn = response.Result.ToBeCompletedOn;
                task.UpdatedAt = response.Result.UpdatedAt;
                task.LocalStatus = LocalStatus.DEFAULT;
                task.ToBeSynced = false;
                result = await _dataService.TaskService.UpdateAsync(task);
            }

            return result;
        }
        #endregion
    }
}