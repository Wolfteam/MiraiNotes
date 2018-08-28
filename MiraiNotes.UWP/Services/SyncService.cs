using AutoMapper;
using MiraiNotes.Data.Models;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.Shared.Models;
using MiraiNotes.UWP.Extensions;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Services
{
    public class SyncService : ISyncService
    {
        //TODO: IMPLEMENT PAGINATION
        //TODO: I think foreachs could be implemented with task.run or something like that
        private readonly IGoogleApiService _apiService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly INetworkService _networkService;
        private readonly IMapper _mapper;

        public SyncService(
            IGoogleApiService apiService,
            IMiraiNotesDataService dataService,
            INetworkService networkService,
            IMapper mapper)
        {
            _apiService = apiService;
            _dataService = dataService;
            _networkService = networkService;
            _mapper = mapper;
        }

        public async Task<EmptyResponse> SyncDownTaskListsAsync(bool isInBackground)
        {
            var syncResult = new EmptyResponse
            {
                Message = string.Empty,
                Succeed = false
            };

            var syncDownResults = new List<EmptyResponse>();

            if (!_networkService.IsInternetAvailable())
            {
                syncResult.Message = $"Network is not available";
                return syncResult;
            }

            var response = await _apiService
                .TaskListService
                .GetAllAsync();

            if (!response.Succeed)
            {
                //TODO: I SHOULD DO SOMETHING HERE...
                syncResult.Message = response.Errors?.ApiError?.Message ?? response.Errors.ErrorDescription;
                return syncResult;
            }

            var downloadedTaskLists = response.Result
                .Items
                .ToList();

            var dbResponse = await _dataService
                .TaskListService
                .GetAllAsNoTrackingAsync();

            if (!dbResponse.Succeed)
            {
                return dbResponse;
            }

            var tasks = new List<Task>
            {
                //Here we save any new remote task list
                Task.Run(async () =>
                {
                    var taskListsToSave = downloadedTaskLists
                        .Where(dt => !dbResponse.Result
                            .Any(ct => ct.GoogleTaskListID == dt.TaskListID))
                        .Select(t => new GoogleTaskList
                        {
                            GoogleTaskListID = t.TaskListID,
                            CreatedAt = DateTime.Now,
                            Title = t.Title,
                            UpdatedAt = t.UpdatedAt
                        });
                    if (taskListsToSave.Count() == 0)
                        return;

                    var r = await _dataService
                        .TaskListService
                        .AddRangeAsync(taskListsToSave);
                    if (r.Succeed)
                        syncDownResults.Add(r);
                }),

                //Here we delete any task that is not in remote
                Task.Run(async() =>
                {
                    var deletedTaskLists = dbResponse.Result
                        .Where(ct => !downloadedTaskLists
                            .Any(dt => dt.TaskListID == ct.GoogleTaskListID));

                    if (deletedTaskLists.Count() == 0)
                        return;

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

                        if (taskList.UpdatedAt < t.UpdatedAt)
                        {
                            taskList.Title = t.Title;
                            taskList.UpdatedAt = t.UpdatedAt;
                            var r = await _dataService
                                .TaskListService
                                .UpdateAsync(taskList);

                            if (r.Succeed)
                                syncDownResults.Add(r);
                        }
                    }
                })
            };

            await Task.WhenAll(tasks);

            if (syncDownResults.Any(r => !r.Succeed))
                syncResult.Message = string.Join(",\n", syncDownResults.Select(r => r.Message));
            else
                syncResult.Succeed = true;

            return syncResult;
        }

        public async Task<EmptyResponse> SyncDownTasksAsync(bool isInBackground)
        {
            var syncResult = new EmptyResponse
            {
                Message = string.Empty,
                Succeed = false
            };

            var syncDownResults = new List<EmptyResponse>();

            if (!_networkService.IsInternetAvailable())
            {
                syncResult.Message = $"Network is not available";
                return syncResult;
            }

            var dbResponse = await _dataService
                .TaskListService
                .GetAllAsNoTrackingAsync();

            foreach (var taskList in dbResponse.Result)
            {
                var response = await _apiService
                    .TaskService
                    .GetAllAsync(taskList.GoogleTaskListID);

                if (!response.Succeed)
                {
                    //TODO: I SHOULD DO SOMETHING HERE TOO...
                    syncResult.Message = response.Errors?.ApiError?.Message ?? response.Errors.ErrorDescription;
                    syncResult.Succeed = false;
                    return syncResult;
                }
                var downloadedTasks = response.Result.Items;

                //if this task list doesnt contains task
                if (downloadedTasks == null || downloadedTasks.Count() == 0)
                    continue;

                //I think i dont need to include the tasklist property
                var currentTasksDbResponse = await _dataService
                   .TaskService
                   .GetAsync(t => t.TaskList.GoogleTaskListID == taskList.GoogleTaskListID, null, string.Empty);

                if (!currentTasksDbResponse.Succeed)
                {
                    return currentTasksDbResponse;
                }

                var tasks = new List<Task>
                {
                    //Here we save any new remote task
                    Task.Run(async() =>
                    {
                        var tasksToSave = downloadedTasks
                            .Where(dt => !currentTasksDbResponse.Result
                                .Any(ct => ct.GoogleTaskID == dt.TaskID))
                            .Select(t => new GoogleTask
                            {
                                GoogleTaskID = t.TaskID,
                                CreatedAt = DateTime.Now,
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
                            });

                        if (tasksToSave.Count() == 0)
                            return;

                        syncDownResults.Add(await _dataService
                            .TaskService
                            .AddRangeAsync(taskList.GoogleTaskListID ,tasksToSave));
                    }),

                    //Here we delete any task that is not in remote
                    Task.Run(async() =>
                    {
                        var deletedTasks = currentTasksDbResponse.Result
                            .Where(ct => !downloadedTasks
                                .Any(dt => dt.TaskID == ct.GoogleTaskID));

                        if (deletedTasks.Count() == 0)
                            return;

                        syncDownResults.Add(await _dataService
                            .TaskService
                            .RemoveRangeAsync(deletedTasks));
                    }),

                    //Here we update the local tasks
                    Task.Run(async() =>
                    {
                        foreach (var task in currentTasksDbResponse.Result)
                        {
                            var downloadedTask = downloadedTasks
                                .FirstOrDefault(dt => dt.TaskID == task.GoogleTaskID);

                            if (downloadedTask == null)
                                return;

                            if (task.UpdatedAt < downloadedTask.UpdatedAt)
                            {
                                task.Title = downloadedTask.Title;
                                task.GoogleTaskID= downloadedTask.TaskID;
                                task.UpdatedAt = downloadedTask.UpdatedAt;

                                syncDownResults.Add( await _dataService
                                    .TaskService
                                    .UpdateAsync(task));
                            }
                        }
                    })
                };

                await Task.WhenAll(tasks);
            }

            if (syncDownResults.Any(r => !r.Succeed))
                syncResult.Message = string.Join(",", syncDownResults.Select(r => r.Message));
            else
                syncResult.Succeed = true;

            return syncResult;
        }

        public async Task<EmptyResponse> SyncUpTaskListsAsync(bool isInBackground)
        {
            var syncUpResult = new EmptyResponse
            {
                Succeed = false,
                Message = string.Empty
            };
            var syncUpResults = new List<EmptyResponse>();

            if (!_networkService.IsInternetAvailable())
            {
                syncUpResult.Message = $"Network is not available";
                return syncUpResult;
            }

            var taskListToSyncDbResponse = await _dataService
                .TaskListService
                .GetAsNoTrackingAsync(
                    taskList => taskList.ToBeSynced,
                    taskList => taskList.OrderBy(tl => tl.UpdatedAt),
                    string.Empty);

            if (!taskListToSyncDbResponse.Succeed)
            {
                return taskListToSyncDbResponse;
            }

            var tasks = new List<Task>
            {
                //Here we take the taskLists that were created
                Task.Run(async() =>
                {
                    var createdTaskLists = taskListToSyncDbResponse.Result
                        .Where(tl => tl.LocalStatus == LocalStatus.CREATED);

                    if (createdTaskLists.Count() == 0)
                        return;

                    foreach (var taskList in createdTaskLists)
                        syncUpResults.Add(await SaveUpTaskList(taskList));
                }),

                //Here we take the tasklists that were deleted
                Task.Run(async() =>
                {
                    var deletedTaskLists = taskListToSyncDbResponse.Result
                        .Where(tl => tl.LocalStatus == LocalStatus.DELETED);

                    if (deletedTaskLists.Count() == 0)
                        return;

                    foreach (var taskList in deletedTaskLists)
                        syncUpResults.Add(await DeleteUpTaskList(taskList));
                }),

                //Here we take the taskLists that were updated
                Task.Run(async() =>
                {
                    var updatedTaskLists = taskListToSyncDbResponse.Result
                        .Where(tl => tl.LocalStatus == LocalStatus.UPDATED);

                    if (updatedTaskLists.Count() == 0)
                        return;

                    foreach (var taskList in updatedTaskLists)
                        syncUpResults.Add(await UpdateUpTaskList(taskList));
                })

                //TODO: WHAT SHOULD I DO WITH MOVE IN A SYNC?
            };

            await Task.WhenAll(tasks);

            if (syncUpResults.Any(r => !r.Succeed))
            {
                syncUpResult.Message = string.Join(
                    ".\n",
                    syncUpResults
                        .Where(r => !r.Succeed)
                        .Select(r => r.Message));
            }
            else
                syncUpResult.Succeed = true;
            return syncUpResult;
        }

        public async Task<EmptyResponse> SyncUpTasksAsync(bool isInBackground)
        {
            var syncUpResult = new EmptyResponse
            {
                Message = string.Empty,
                Succeed = false
            };

            var syncUpResults = new List<EmptyResponse>();

            if (!_networkService.IsInternetAvailable())
            {
                syncUpResult.Message = $"Network is not available";
                return syncUpResult;
            }

            var tasksToBeSyncedDbResponse = await _dataService
                .TaskService
                .GetAsNoTrackingAsync(
                    task => task.ToBeSynced,
                    task => task.OrderBy(t => t.UpdatedAt),
                    nameof(GoogleTask.TaskList));

            if (!tasksToBeSyncedDbResponse.Succeed)
                return tasksToBeSyncedDbResponse;

            var tasks = new List<Task>
            {
                //Here we save the tasks that were created locally
                Task.Run(async() =>
                {
                    var tasksToCreate = tasksToBeSyncedDbResponse.Result
                        .Where(t => t.LocalStatus == LocalStatus.CREATED);

                    if (tasksToCreate.Count() == 0)
                        return;

                    foreach (var task in tasksToCreate)
                    {
                        string localID = task.GoogleTaskID;
                        var result = await SaveUpTask(task);
                        syncUpResults.Add(result);

                        if (!result.Succeed)
                            continue;

                        //If this isn't a sub task
                        if (string.IsNullOrEmpty(task.ParentTask))
                        {
                            tasksToCreate
                                .Where(st => st.ParentTask == localID)
                                .ForEach(st => st.ParentTask = task.GoogleTaskID);
                        }
                        //If this is a sub task and there are sts whose position depends in the current one
                        //we need to update them
                        else if (tasksToCreate.Any(st => st.Position == localID))
                        {
                            //In theory this should only affect 1 st
                            tasksToCreate
                                .Where(st => st.Position == localID)
                                .ForEach(st => st.Position = task.GoogleTaskID);
                        }
                    }
                }),

                //Here we save the tasks that were deleted locally
                Task.Run(async() =>
                {
                    var tasksToDelete = tasksToBeSyncedDbResponse.Result
                        .Where(t => t.LocalStatus == LocalStatus.DELETED);
                    foreach (var task in tasksToDelete)
                        syncUpResults.Add(await DeleteUpTask(task));
                }),

                //Here we update the tasks that were updated locally
                Task.Run(async()=>
                {
                    var tasksToUpdate = tasksToBeSyncedDbResponse.Result
                        .Where(t => t.LocalStatus == LocalStatus.UPDATED);

                    foreach (var task in tasksToUpdate)
                        syncUpResults.Add(await UpdateUpTask(task));
                })
            };

            await Task.WhenAll(tasks);

            if (syncUpResults.Any(r => !r.Succeed))
            {
                syncUpResult.Message = string.Join(
                    ".\n",
                    syncUpResults
                        .Where(r => !r.Succeed)
                        .Select(r => r.Message));
            }
            else
                syncUpResult.Succeed = true;
            return syncUpResult;
        }



        private async Task<EmptyResponse> SaveUpTaskList(GoogleTaskList taskList)
        {
            var response = await _apiService.TaskListService.SaveAsync(new GoogleTaskListModel
            {
                Title = taskList.Title,
                UpdatedAt = taskList.UpdatedAt
            });

            var result = new EmptyResponse
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
                result.Message = response.Errors?.ApiError?.Message ??
                $"An unkwon error occurred while trying to create task list {taskList.Title}";

            return result;
        }

        private async Task<EmptyResponse> DeleteUpTaskList(GoogleTaskList taskList)
        {
            var response = await _apiService
                .TaskListService
                .DeleteAsync(taskList.GoogleTaskListID);

            var result = new EmptyResponse
            {
                Succeed = response.Succeed
            };

            if (response.Succeed)
                result = await _dataService.TaskListService.RemoveAsync(taskList);
            else
                result.Message = response.Errors?.ApiError?.Message ??
                    $"An unkwon error occurred while trying to delete task list {taskList.Title}";

            return result;
        }

        private async Task<EmptyResponse> UpdateUpTaskList(GoogleTaskList taskList)
        {
            var response = await _apiService
                .TaskListService
                .GetAsync(taskList.GoogleTaskListID);

            var result = new EmptyResponse
            {
                Succeed = response.Succeed
            };
            if (!response.Succeed)
            {
                result.Message = response.Errors?.ApiError?.Message ??
                    $"An unknow error occurred while trying to get {taskList.Title} from remote to be updated";
            }
            else
            {
                //We need to update the remote contrapart
                if (taskList.UpdatedAt > response.Result.UpdatedAt)
                {
                    response.Result.UpdatedAt = taskList.UpdatedAt;
                    response.Result.Title = taskList.Title;
                    response = await _apiService
                        .TaskListService
                        .UpdateAsync(response.Result.TaskListID, response.Result);

                    result.Succeed = response.Succeed;

                    if (!response.Succeed)
                        result.Message = response.Errors?.ApiError?.Message ??
                           $"An unknow error occurred while trying to get {taskList.Title} from remote to be updated";
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
            }

            return result;
        }



        private async Task<EmptyResponse> SaveUpTask(GoogleTask task)
        {
            var t = new GoogleTaskModel
            {
                Notes = task.Notes,
                Status = task.Status,
                Title = task.Title,
                ToBeCompletedOn = task.ToBeCompletedOn,
                UpdatedAt = task.UpdatedAt
            };

            var response = await _apiService
                .TaskService
                .SaveAsync(task.TaskList.GoogleTaskListID, t, task.ParentTask, task.Position);

            var result = new EmptyResponse
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
                result.Message = response.Errors?.ApiError?.Message ??
                    $"An unkwon error occurred while trying to create task {task.Title}";

            return result;
        }

        private async Task<EmptyResponse> DeleteUpTask(GoogleTask task)
        {
            var response = await _apiService
                .TaskService
                .DeleteAsync(task.TaskList.GoogleTaskListID, task.GoogleTaskID);

            var result = new EmptyResponse
            {
                Succeed = response.Succeed
            };

            if (response.Succeed)
                result = await _dataService.TaskService.RemoveAsync(task);
            else
                result.Message = response.Errors?.ApiError?.Message ??
                    $"An unkwon error occurred while trying to delete task {task.Title}";

            return result;
        }

        private async Task<EmptyResponse> UpdateUpTask(GoogleTask task)
        {
            var response = await _apiService
                .TaskService
                .GetAsync(task.TaskList.GoogleTaskListID, task.GoogleTaskID);

            var result = new EmptyResponse
            {
                Succeed = response.Succeed
            };
            if (!response.Succeed)
            {
                result.Message = response.Errors?.ApiError?.Message ??
                    $"An unkwon error occurred while trying to get task {task.Title}";
            }
            else
            {
                if (task.UpdatedAt > response.Result.UpdatedAt)
                {
                    response.Result.CompletedOn = task.CompletedOn;
                    response.Result.IsDeleted = task.IsDeleted;
                    response.Result.Notes = task.Notes;
                    response.Result.Status = task.Status;
                    response.Result.Title = task.Title;
                    response.Result.ToBeCompletedOn = task.ToBeCompletedOn;
                    response.Result.UpdatedAt = task.UpdatedAt;

                    response = await _apiService
                        .TaskService
                        .UpdateAsync(task.TaskList.GoogleTaskListID, task.GoogleTaskID, response.Result);

                    result.Succeed = response.Succeed;

                    if (response.Succeed)
                    {
                        task.LocalStatus = LocalStatus.DEFAULT;
                        task.ToBeSynced = false;
                        result = await _dataService.TaskService.UpdateAsync(task);
                    }
                    else
                        result.Message = response.Errors?.ApiError?.Message ??
                            $"An unkwon error occurred while trying to delete task {task.Title}";
                }
                else
                {
                    task.CompletedOn = response.Result.CompletedOn;
                    task.IsDeleted = response.Result.IsDeleted;
                    task.Notes = response.Result.Notes;
                    task.Status = response.Result.Status;
                    task.Title = response.Result.Title;
                    task.ToBeCompletedOn = response.Result.ToBeCompletedOn;
                    task.UpdatedAt = response.Result.UpdatedAt;
                    task.LocalStatus = LocalStatus.DEFAULT;
                    task.ToBeSynced = false;
                    result = await _dataService.TaskService.UpdateAsync(task);
                }
            }

            return result;
        }
    }
}
