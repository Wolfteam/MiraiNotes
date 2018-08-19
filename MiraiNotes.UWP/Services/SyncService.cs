using AutoMapper;
using MiraiNotes.Data.Models;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.Shared.Models;
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
        private readonly IMapper _mapper;

        public SyncService(
            IGoogleApiService apiService, 
            IMiraiNotesDataService dataService, 
            IMapper mapper)
        {
            //TODO: CHECK INTERNET CONNCTION IN THE API SERVICE
            _apiService = apiService;
            _dataService = dataService;
            _mapper = mapper;
        }

        public async Task<Result> SyncDownTaskListsAsync(bool isInBackground)
        {
            var syncResult = new Result
            {
                Message = string.Empty,
                Succeed = false
            };

            var response = await _apiService
                .TaskListService
                .GetAllAsync();

            if (!response.Succeed)
            {
                //TODO: I SHOULD DO SOMETHING HERE...
                syncResult.Message = response.Errors?.ApiError?.Message ?? response.Errors.ErrorDescription;
                return syncResult;
            }

            var currentActiveUser = await _dataService
                .UserService
                .GetCurrentActiveUserAsync();

            if (currentActiveUser == null)
            {
                syncResult.Message = "The current active user couldnt be found on db";
                return syncResult;
            }

            var downloadedTaskLists = response.Result
                .Items
                .ToList();

            var currentTaskLists = await _dataService
                .TaskListService
                .GetAllAsNoTrackingAsync();

            var tasks = new List<Task>
            {
                //Here we save any new remote task list
                Task.Run(async () =>
                {
                    var taskListsToSave = downloadedTaskLists
                        .Where(dt => !currentTaskLists
                            .Any(ct => ct.GoogleTaskListID == dt.TaskListID))
                        .Select(t => new GoogleTaskList
                        {
                            GoogleTaskListID = t.TaskListID,
                            CreatedAt = DateTime.Now,
                            Title = t.Title,
                            UpdatedAt = t.UpdatedAt,
                            User = currentActiveUser
                        });

                    await _dataService
                        .TaskListService
                        .AddRangeAsync(taskListsToSave);
                }),

                //Here we delete any task that is not in remote
                Task.Run(() =>
                {
                    var deletedTaskLists = currentTaskLists
                        .Where(ct => !downloadedTaskLists
                            .Any(dt => dt.TaskListID == ct.GoogleTaskListID));

                    _dataService.TaskListService.RemoveRange(deletedTaskLists);
                }),

                //Here we update the local tasklists
                Task.Run(async () =>
                {
                    foreach (var taskList in currentTaskLists)
                    {
                        var t = downloadedTaskLists
                            .FirstOrDefault(dt => dt.TaskListID == taskList.GoogleTaskListID);

                        if (t == null)
                            return;

                        if (taskList.UpdatedAt < t.UpdatedAt)
                        {
                            var taskToUpdate = await _dataService
                                .TaskListService
                                .GetByIdAsync(taskList.ID);

                            if (taskToUpdate == null)
                                continue;

                            taskToUpdate.Title = t.Title;
                            taskToUpdate.GoogleTaskListID = t.TaskListID;
                            taskToUpdate.UpdatedAt = t.UpdatedAt;
                            _dataService.TaskListService.Update(taskToUpdate);
                        }
                    }
                })
            };

            await Task.WhenAll(tasks);

            syncResult = await _dataService.SaveChangesAsync();

            return syncResult;
        }

        public async Task<Result> SyncDownTasksAsync(bool isInBackground)
        {
            var syncResult = new Result
            {
                Message = string.Empty,
                Succeed = true
            };

            var taskLists = await _dataService
                .TaskListService
                .GetAllAsync();

            foreach (var taskList in taskLists)
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

                //I think i dont need to include the tasklist property
                var currentTasks = (await _dataService
                   .TaskService
                   .GetAsync(t => t.TaskList.GoogleTaskListID == taskList.GoogleTaskListID, null, string.Empty))
                   .ToList();

                var tasks = new List<Task>
                {
                    //Here we save any new remote task
                    Task.Run(async() =>
                    {
                        var tasksToSave = downloadedTasks
                            .Where(dt => !currentTasks
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
                                ToBeCompletedOn = t.ToBeCompletedOn,
                                TaskList = taskList
                            });

                        await _dataService
                            .TaskService
                            .AddRangeAsync(tasksToSave);
                    }),

                    //Here we delete any task that is not in remote
                    Task.Run(() =>
                    {
                        var deletedTasks = currentTasks
                            .Where(ct => !downloadedTasks
                                .Any(dt => dt.TaskID == ct.GoogleTaskID));

                        _dataService.TaskService.RemoveRange(deletedTasks);
                    }),

                    //Here we update the local tasklists
                    Task.Run(async() =>
                    {
                        foreach (var task in currentTasks)
                        {
                            var t = downloadedTasks
                                .FirstOrDefault(dt => dt.TaskID == task.GoogleTaskID);

                            if (t == null)
                                return;

                            if (taskList.UpdatedAt < t.UpdatedAt)
                            {
                                var taskToUpdate = await _dataService
                                    .TaskListService
                                    .GetByIdAsync(taskList.ID);

                                if (taskToUpdate == null)
                                    continue;

                                taskToUpdate.Title = t.Title;
                                taskToUpdate.GoogleTaskListID = t.TaskID;
                                taskToUpdate.UpdatedAt = t.UpdatedAt;

                                _dataService.TaskListService.Update(taskToUpdate);
                            }
                        }
                    })
                };

                await Task.WhenAll(tasks);

                syncResult = await _dataService.SaveChangesAsync();
                if (!syncResult.Succeed)
                {
                    return syncResult;
                }
            }

            return syncResult;
        }

        public async Task<Result> SyncUpTaskListsAsync(bool isInBackground)
        {
            var syncUpResult = new Result
            {
                Succeed = false
            };
            var syncUpResults = new List<Result>();

            var taskListToSync = await _dataService
                .TaskListService
                .GetAsync(
                    taskList => taskList.ToBeSynced,
                    taskList => taskList.OrderBy(tl => tl.UpdatedAt),
                    string.Empty);


            var tasks = new List<Task>
            {
                //Here we take the taskLists that were created
                Task.Run(async() =>
                {
                    var createdTaskLists = taskListToSync
                        .Where(tl => tl.LocalStatus == LocalStatus.CREATED);

                    if (createdTaskLists.Count() == 0)
                        return;

                    foreach (var taskList in createdTaskLists)
                    {
                        syncUpResults.Add(await SaveUpTaskList(taskList));
                    }
                    var dbResult = await _dataService.SaveChangesAsync();
                    syncUpResults.Add(dbResult);
                }),

                //Here we take the tasklists that were deleted
                Task.Run(async() =>
                {
                    var deletedTaskLists = taskListToSync
                        .Where(tl => tl.LocalStatus == LocalStatus.DELETED);

                    if (deletedTaskLists.Count() == 0)
                        return;

                    foreach (var taskList in deletedTaskLists)
                    {
                        syncUpResults.Add(await DeleteUpTaskList(taskList));
                    }

                    var dbResult = await _dataService.SaveChangesAsync();
                    syncUpResults.Add(dbResult);
                }),

                //Here we take the taskLists that were updated
                Task.Run(async() =>
                {
                    var updatedTaskLists = taskListToSync
                        .Where(tl => tl.LocalStatus == LocalStatus.UPDATED);

                    if (updatedTaskLists.Count() == 0)
                        return;

                    foreach (var taskList in updatedTaskLists)
                    {
                        syncUpResults.Add(await UpdateUpTaskList(taskList));
                    }
                    var dbResult = await _dataService.SaveChangesAsync();
                    syncUpResults.Add(dbResult);
                })

                //TODO: WHAT SHOULD I DO WITH MOVE IN A SYNC?
            };

            if (syncUpResults.Any(r => !r.Succeed))
            {
                syncUpResult.Message = string.Join(
                    ".\n",
                    syncUpResults
                        .Where(r => !r.Succeed)
                        .Select(r => r.Message));
            }
            return syncUpResult;
        }

        public async Task<Result> SyncUpTasksAsync(bool isInBackground)
        {
            var syncUpResult = new Result
            {
                Message = string.Empty,
                Succeed = true
            };

            var syncUpResults = new List<Result>();

            var tasksToBeSynced = await _dataService
                .TaskService
                .GetAsync(
                    task => task.ToBeSynced,
                    task => task.OrderBy(t => t.UpdatedAt),
                    nameof(GoogleTask.TaskList));

            var tasks = new List<Task>
            {
                //Here we save the tasks that were created locally
                Task.Run(async() =>
                {
                    var tasksToCreate = tasksToBeSynced
                        .Where(t => t.LocalStatus == LocalStatus.CREATED);

                    if (tasksToCreate.Count() == 0)
                        return;

                    foreach (var task in tasksToCreate)
                    {
                        syncUpResults.Add(await SaveUpTask(task));
                    }

                    syncUpResults.Add(await _dataService.SaveChangesAsync());
                }),

                //Here we save the tasks that were deleted locally
                Task.Run(async() =>
                {
                    var tasksToDelete = tasksToBeSynced
                        .Where(t => t.LocalStatus == LocalStatus.DELETED);
                    foreach (var task in tasksToDelete)
                    {
                        syncUpResults.Add(await DeleteUpTask(task));
                    }
                    syncUpResults.Add(await _dataService.SaveChangesAsync());
                }),

                //Here we update the tasks that were updated locally
                Task.Run(async()=>
                {
                    var tasksToUpdate = tasksToBeSynced
                        .Where(t => t.LocalStatus == LocalStatus.UPDATED);

                    foreach (var task in tasksToUpdate)
                    {
                        syncUpResults.Add(await UpdateUpTask(task));
                    }
                    syncUpResults.Add(await _dataService.SaveChangesAsync());
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
            return syncUpResult;
        }



        private async Task<Result> SaveUpTaskList(GoogleTaskList taskList)
        {
            var response = await _apiService.TaskListService.SaveAsync(new GoogleTaskListModel
            {
                Title = taskList.Title,
                UpdatedAt = taskList.UpdatedAt
            });

            var result = new Result
            {
                Succeed = response.Succeed
            };
            if (response.Succeed)
            {
                taskList.GoogleTaskListID = response.Result.TaskListID;
                taskList.ToBeSynced = false;
                taskList.LocalStatus = LocalStatus.DEFAULT;
                _dataService.TaskListService.Update(taskList);
            }
            else
                result.Message = response.Errors?.ApiError?.Message ??
                $"An unkwon error occurred while trying to create task list {taskList.Title}";

            return result;
        }

        private async Task<Result> DeleteUpTaskList(GoogleTaskList taskList)
        {
            var response = await _apiService
                .TaskListService
                .DeleteAsync(taskList.GoogleTaskListID);

            var result = new Result
            {
                Succeed = response.Succeed
            };

            if (response.Succeed)
                _dataService.TaskListService.Remove(taskList);
            else
                result.Message = response.Errors?.ApiError?.Message ??
                    $"An unkwon error occurred while trying to delete task list {taskList.Title}";

            return result;
        }

        private async Task<Result> UpdateUpTaskList(GoogleTaskList taskList)
        {
            var response = await _apiService
                .TaskListService
                .GetAsync(taskList.GoogleTaskListID);

            var result = new Result
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
                        _dataService.TaskListService.Update(taskList);
                    }
                }
                //we need to update the local contrapart
                else
                {
                    taskList.Title = response.Result.Title;
                    taskList.LocalStatus = LocalStatus.DEFAULT;
                    taskList.ToBeSynced = false;
                    _dataService.TaskListService.Update(taskList);
                }
            }

            return result;
        }



        private async Task<Result> SaveUpTask(GoogleTask task)
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

            var result = new Result
            {
                Succeed = response.Succeed
            };

            if (response.Succeed)
            {
                task.LocalStatus = LocalStatus.DEFAULT;
                task.ToBeSynced = false;
                task.GoogleTaskID = response.Result.TaskID;
                task.UpdatedAt = response.Result.UpdatedAt;
                _dataService.TaskService.Update(task);
            }
            else
                result.Message = response.Errors?.ApiError?.Message ??
                    $"An unkwon error occurred while trying to create task {task.Title}";

            return result;
        }

        private async Task<Result> DeleteUpTask(GoogleTask task)
        {
            var response = await _apiService
                .TaskService
                .DeleteAsync(task.TaskList.GoogleTaskListID, task.GoogleTaskID);

            var result = new Result
            {
                Succeed = response.Succeed
            };

            if (response.Succeed)
                _dataService.TaskService.Remove(task);
            else
                result.Message = response.Errors?.ApiError?.Message ??
                    $"An unkwon error occurred while trying to delete task {task.Title}";

            return result;
        }

        private async Task<Result> UpdateUpTask(GoogleTask task)
        {
            var response = await _apiService
                .TaskService
                .GetAsync(task.TaskList.GoogleTaskListID, task.GoogleTaskID);

            var result = new Result
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
                        _dataService.TaskService.Update(task);
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
                    _dataService.TaskService.Update(task);
                }
            }

            return result;
        }
    }
}
