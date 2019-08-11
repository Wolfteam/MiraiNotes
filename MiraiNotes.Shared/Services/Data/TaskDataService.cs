using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MiraiNotes.Shared.Helpers;
using Serilog;

namespace MiraiNotes.Shared.Services.Data
{
    public class TaskDataService : ITaskDataService
    {
        private readonly ILogger _logger;
        public TaskDataService(ILogger logger)
        {
            _logger = logger.ForContext<TaskDataService>();
        }

        public async Task<ResponseDto<GoogleTask>> AddAsync(GoogleTask entity)
        {
            return await Task.Run(async () =>
            {
                _logger.Information("AddAsync: Trying to add a new task {@Task}", entity);
                var response = new ResponseDto<GoogleTask>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        await context.AddAsync(entity);
                        response.Succeed = await context.SaveChangesAsync() > 0;
                        response.Result = entity;
                        _logger.Information("AddAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "AddAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<GoogleTask>> AddAsync(string taskListID, GoogleTask task)
        {
            return await Task.Run(async () =>
            {
                _logger.Information("AddAsync: Trying to add a new task {@Task}", task);
                var response = new ResponseDto<GoogleTask>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        var taskList = await context.TaskLists
                            .FirstOrDefaultAsync(tl => tl.GoogleTaskListID == taskListID);

                        if (taskList == null)
                        {
                            response.Message = $"Couldn't find the tasklist where {task.Title} is going to be saved";
                            _logger.Warning($"AddAsync: Couldn't find a tasklist with id = {taskListID}");
                        }
                        else
                        {
                            task.TaskList = taskList;
                            await context.AddAsync(task);
                            response.Succeed = await context.SaveChangesAsync() > 0;
                            response.Result = task;
                        }
                        _logger.Information("AddAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "AddAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<IEnumerable<GoogleTask>>> AddRangeAsync(IEnumerable<GoogleTask> entities)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"AddRangeAsync: Trying to add {entities.Count()} tasks");
                var response = new ResponseDto<IEnumerable<GoogleTask>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        await context.AddRangeAsync(entities);
                        response.Succeed = await context.SaveChangesAsync() > 0;
                        response.Result = entities;
                        _logger.Information("AddRangeAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "AddRangeAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<IEnumerable<GoogleTask>>> AddRangeAsync(string taskListID, IEnumerable<GoogleTask> tasks)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"AddRangeAsync: Trying to add {tasks.Count()} tasks to tasklist = {taskListID}");
                var response = new ResponseDto<IEnumerable<GoogleTask>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        var taskList = await context.TaskLists
                            .FirstOrDefaultAsync(tl => tl.GoogleTaskListID == taskListID);

                        if (taskList == null)
                        {
                            _logger.Warning($"AddRangeAsync: Couldn't find a tasklist with id = {taskListID}");
                            response.Message = $"Couldn't find the tasklist where all the tasks will be saved in the db";
                        }
                        else
                        {
                            var entities = tasks.ToList();
                            entities.ForEach(t => t.TaskList = taskList);

                            await context.AddRangeAsync(entities);
                            response.Succeed = await context.SaveChangesAsync() > 0;
                            response.Result = entities;
                        }
                        _logger.Information("AddRangeAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "AddRangeAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<bool>> ExistsAsync(Expression<Func<GoogleTask, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"ExistsAsync: Trying to find the first task that matches {predicate.ToString()}");
                var response = new ResponseDto<bool>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context.Tasks
                            .AsNoTracking()
                            .Where(predicate)
                            .CountAsync() == 1;
                        response.Succeed = true;
                        _logger.Information("ExistsAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "ExistsAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<GoogleTask>> FirstOrDefaultAsync(Expression<Func<GoogleTask, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"FirstOrDefaultAsync: Trying to find the first task that matches {predicate.ToString()}");
                var response = new ResponseDto<GoogleTask>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .Tasks
                            .FirstOrDefaultAsync(predicate);
                        response.Succeed = true;
                        _logger.Information("FirstOrDefaultAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "FirstOrDefaultAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<GoogleTask>> FirstOrDefaultAsNoTrackingAsync(Expression<Func<GoogleTask, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"FirstOrDefaultAsNoTrackingAsync: Trying to find the first task that matches {predicate.ToString()}");
                var response = new ResponseDto<GoogleTask>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .Tasks
                            .AsNoTracking()
                            .FirstOrDefaultAsync(predicate);
                        response.Succeed = true;
                        _logger.Information("FirstOrDefaultAsNoTrackingAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "FirstOrDefaultAsNoTrackingAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<IEnumerable<GoogleTask>>> GetAllAsNoTrackingAsync()
        {
            return await Task.Run(async () =>
            {
                _logger.Information("GetAllAsNoTrackingAsync: Trying to get all the task");
                var response = new ResponseDto<IEnumerable<GoogleTask>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .Tasks
                            .AsNoTracking()
                            .ToListAsync();
                        response.Succeed = true;
                        _logger.Information("GetAllAsNoTrackingAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "GetAllAsNoTrackingAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<IEnumerable<GoogleTask>>> GetAllAsync()
        {
            return await Task.Run(async () =>
            {
                _logger.Information("GetAllAsync: Trying to get all the task");
                var response = new ResponseDto<IEnumerable<GoogleTask>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .Tasks
                            .ToListAsync();
                        response.Succeed = true;
                        _logger.Information("GetAllAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "GetAllAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<IEnumerable<GoogleTask>>> GetAsNoTrackingAsync(Expression<Func<GoogleTask, bool>> filter = null, Func<IQueryable<GoogleTask>, IOrderedQueryable<GoogleTask>> orderBy = null, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                _logger.Information("GetAsNoTrackingAsync: Getting all tasks");
                var response = new ResponseDto<IEnumerable<GoogleTask>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        IQueryable<GoogleTask> query = context.Tasks;
                        if (filter != null)
                        {
                            query = query.Where(filter);
                        }

                        foreach (var includeProperty in includeProperties.Split
                            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            query = query.Include(includeProperty.Trim());
                        }

                        if (orderBy != null)
                        {
                            response.Result = await orderBy(query)
                                .AsNoTracking()
                                .ToListAsync();
                        }
                        else
                        {
                            response.Result = await query
                                .AsNoTracking()
                                .ToListAsync();
                        }
                        response.Succeed = true;
                        _logger.Information("GetAsNoTrackingAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "GetAsNoTrackingAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<IEnumerable<GoogleTask>>> GetAsync(Expression<Func<GoogleTask, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"GetAsync: Getting all tasks with {predicate.ToString()}");
                var response = new ResponseDto<IEnumerable<GoogleTask>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                             .Tasks
                             .Where(predicate)
                             .ToListAsync();
                        response.Succeed = true;
                        _logger.Information("GetAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "GetAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<GoogleTask>> GetAsync(Expression<Func<GoogleTask, bool>> filter, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"GetAsync: Getting all tasks with {filter.ToString()}");
                var response = new ResponseDto<GoogleTask>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        IQueryable<GoogleTask> query = context.Tasks;
                        if (filter != null)
                        {
                            query = query.Where(filter);
                        }

                        foreach (var includeProperty in includeProperties.Split
                            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            query = query.Include(includeProperty.Trim());
                        }

                        response.Result = await query.FirstOrDefaultAsync();
                        response.Succeed = true;
                        _logger.Information("GetAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "GetAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<IEnumerable<GoogleTask>>> GetAsync(Expression<Func<GoogleTask, bool>> filter = null, Func<IQueryable<GoogleTask>, IOrderedQueryable<GoogleTask>> orderBy = null, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"GetAsync: Getting all tasks with {filter.ToString()}");
                var response = new ResponseDto<IEnumerable<GoogleTask>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        IQueryable<GoogleTask> query = context.Tasks;
                        if (filter != null)
                        {
                            query = query.Where(filter);
                        }

                        foreach (var includeProperty in includeProperties.Split
                            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            query = query.Include(includeProperty.Trim());
                        }

                        if (orderBy != null)
                        {
                            response.Result = await orderBy(query).ToListAsync();
                        }
                        else
                        {
                            response.Result = await query.ToListAsync();
                        }
                        response.Succeed = true;
                        _logger.Information("GetAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "GetAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<GoogleTask>> GetByIdAsync(object id)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"GetByIdAsync: Looking for task with id = {id}");
                var response = new ResponseDto<GoogleTask>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .Tasks
                            .FindAsync(id);
                        response.Succeed = true;
                        _logger.Information("GetByIdAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "GetByIdAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponseDto> RemoveAsync(GoogleTask entity)
        {
            return await Task.Run(async () =>
            {
                _logger.Information("RemoveAsync: Trying to delete {@Task}", entity);
                var response = new EmptyResponseDto
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        context.Remove(entity);
                        response.Succeed = await context.SaveChangesAsync() > 0;
                        _logger.Information("RemoveAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "RemoveAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponseDto> RemoveAsync(object id)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"RemoveAsync: Trying to delete task with id {id}");
                var response = new EmptyResponseDto
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        var entity = await context.Tasks.FindAsync(id);
                        if (entity == null)
                        {
                            response.Message = "Entity couldn't be removed cause it wasnt found";
                            _logger.Warning("RemoveAsync: Task couldn't be removed cause it wasnt found");
                        }
                        else
                        {
                            context.Remove(entity);
                            response.Succeed = await context.SaveChangesAsync() > 0;
                        }
                        _logger.Information("RemoveAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "RemoveAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponseDto> RemoveAsync(Expression<Func<GoogleTask, bool>> filter)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"RemoveAsync: Trying to delete tasks that matches {filter.ToString()}");
                var response = new EmptyResponseDto
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        var entitiesToDelete = context
                             .Tasks
                             .Where(filter);

                        if (entitiesToDelete.Count() > 0)
                        {
                            context.RemoveRange(entitiesToDelete);
                            response.Succeed = await context.SaveChangesAsync() > 0;
                        }
                        else
                            response.Succeed = true;
                        _logger.Information("RemoveAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "RemoveAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponseDto> RemoveRangeAsync(IEnumerable<GoogleTask> entities)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"RemoveRangeAsync: Trying to delete {entities.Count()} tasks");
                var response = new EmptyResponseDto
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        context.RemoveRange(entities);
                        response.Succeed = await context.SaveChangesAsync() > 0;
                        _logger.Information("RemoveRangeAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "RemoveRangeAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<GoogleTask>> UpdateAsync(GoogleTask entity)
        {
            return await Task.Run(async () =>
            {
                _logger.Information("UpdateAsync: Trying to update {@Task}", entity);
                var response = new ResponseDto<GoogleTask>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        context.Entry(entity).State = EntityState.Modified;
                        response.Succeed = await context.SaveChangesAsync() > 0;
                        response.Result = entity;
                        _logger.Information("UpdateAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "UpdateAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<IEnumerable<GoogleTask>>> UpdateRangeAsync(IEnumerable<GoogleTask> entities)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"UpdateRangeAsync: Trying to update {entities.Count()} tasks");
                var response = new ResponseDto<IEnumerable<GoogleTask>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        foreach (var entity in entities)
                        {
                            context.Entry(entity).State = EntityState.Modified;
                        }
                        response.Succeed = await context.SaveChangesAsync() > 0;
                        response.Result = entities;
                        _logger.Information("UpdateRangeAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "UpdateRangeAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<GoogleTask>> MoveAsync(string selectedTaskListID, string taskID, string parentTask, string previous)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"MoveAsync: Trying to move the taskID = {taskID} to tasklistID {selectedTaskListID} with parentTask = {parentTask} and previousTaskID = {previous}");
                var response = new ResponseDto<GoogleTask>
                {
                    Message = string.Empty,
                    Succeed = false
                };

                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        var taskList = await context
                            .TaskLists
                            .FirstOrDefaultAsync(tl => tl.GoogleTaskListID == selectedTaskListID);

                        if (taskList == null)
                        {
                            response.Message = $"Couldn't find the selected task list in db";
                            _logger.Warning($"MoveAsync: Couldn't find the selected task list id = {selectedTaskListID}");
                            return response;
                        }

                        var oldEntity = await context
                            .Tasks
                            .FirstOrDefaultAsync(t => t.GoogleTaskID == taskID);

                        if (oldEntity == null)
                        {
                            response.Message = $"Couldn't find the task to be moved in db";
                            _logger.Warning($"MoveAsync: Couldn't find the task to be moved. TaskID = {taskID}");
                            return response;
                        }

                        //if this isnt a sub task
                        if (string.IsNullOrEmpty(oldEntity.ParentTask))
                        {
                            previous = context.Tasks
                                .Where(
                                    t => t.TaskList == taskList &&
                                    t.LocalStatus != LocalStatus.CREATED &&
                                    string.IsNullOrEmpty(t.ParentTask))
                                .OrderBy(t => t.Position)
                                .LastOrDefault()?.GoogleTaskID;
                        }

                        var entity = new GoogleTask
                        {
                            CompletedOn = oldEntity.CompletedOn,
                            CreatedAt = DateTimeOffset.UtcNow,
                            GoogleTaskID = Guid.NewGuid().ToString(),
                            IsDeleted = oldEntity.IsDeleted,
                            IsHidden = oldEntity.IsHidden,
                            LocalStatus = LocalStatus.CREATED,
                            Notes = oldEntity.Notes,
                            ParentTask = parentTask,
                            Position = previous,
                            RemindOn = oldEntity.RemindOn,
                            RemindOnGUID = oldEntity.RemindOnGUID,
                            Status = oldEntity.Status,
                            TaskList = taskList,
                            Title = oldEntity.Title,
                            ToBeCompletedOn = oldEntity.ToBeCompletedOn,
                            ToBeSynced = true,
                            UpdatedAt = DateTimeOffset.UtcNow
                        };

                        if (oldEntity.LocalStatus == LocalStatus.CREATED)
                            context.Remove(oldEntity);
                        else
                        {
                            oldEntity.LocalStatus = LocalStatus.DELETED;
                            oldEntity.ToBeSynced = true;
                            oldEntity.UpdatedAt = DateTimeOffset.UtcNow;
                        }

                        await context.Tasks.AddAsync(entity);

                        response.Succeed = await context.SaveChangesAsync() > 0;
                        response.Result = entity;
                        _logger.Information("MoveAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "MoveAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponseDto> RemoveTaskAsync(string taskID)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"RemoveTaskAsync: Trying to remove taskID = {taskID}");
                var response = new EmptyResponseDto
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        var entity = await context
                            .Tasks
                            .FirstOrDefaultAsync(t => t.GoogleTaskID == taskID);

                        if (entity == null)
                        {
                            response.Message = "Couldn't find the task to delete";
                            _logger.Warning($"RemoveTaskAsync: Couldn't find the task with taskID = {taskID}");
                            return response;
                        }

                        var subTasks = context
                            .Tasks
                            .Where(t => t.ParentTask == entity.GoogleTaskID);

                        if (entity.LocalStatus == LocalStatus.CREATED)
                        {
                            context.Remove(entity);
                            if (subTasks.Count() > 0)
                                context.RemoveRange(subTasks);
                        }
                        else
                        {
                            entity.LocalStatus = LocalStatus.DELETED;
                            entity.UpdatedAt = DateTimeOffset.UtcNow;
                            entity.ToBeSynced = true;

                            context.Update(entity);

                            if (subTasks.Count() > 0)
                            {
                                await subTasks.ForEachAsync(st =>
                                {
                                    st.LocalStatus = LocalStatus.DELETED;
                                    st.UpdatedAt = DateTimeOffset.UtcNow;
                                    st.ToBeSynced = true;
                                });
                                context.UpdateRange(subTasks);
                            }
                        }
                        response.Succeed = await context.SaveChangesAsync() > 0;
                        _logger.Information("RemoveTaskAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "RemoveTaskAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponseDto> RemoveTaskAsync(IEnumerable<string> taskIds)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"RemoveTaskAsync: Trying to remove following taskIDs = {string.Join(",", taskIds)}");
                var response = new EmptyResponseDto
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        var entities = context
                            .Tasks
                            .Where(t => taskIds.Any(a => a == t.GoogleTaskID));

                        if (entities.Count() == 0)
                        {
                            response.Message = "Couldn't find the tasks to delete";
                            _logger.Warning($"RemoveTaskAsync: Couldn't find any of the taskIDs = {string.Join(",", taskIds)}");
                            return response;
                        }

                        if (entities.Any(t => t.LocalStatus == LocalStatus.CREATED))
                        {
                            var tasksToDelete = entities.Where(t => t.LocalStatus == LocalStatus.CREATED);
                            var subTasks = context
                                .Tasks
                                .Where(st => tasksToDelete.Any(t => st.ParentTask == t.GoogleTaskID));
                            context.RemoveRange(tasksToDelete);
                            if (subTasks.Count() > 0)
                                context.RemoveRange(subTasks);
                        }

                        if (entities.Any(t => t.LocalStatus != LocalStatus.CREATED))
                        {
                            var tasksToUpdate = entities.Where(t => t.LocalStatus != LocalStatus.CREATED);
                            var subTasks = context
                                .Tasks
                                .Where(st => tasksToUpdate.Any(t => st.ParentTask == t.GoogleTaskID));

                            await tasksToUpdate.ForEachAsync(t =>
                            {
                                t.UpdatedAt = DateTimeOffset.UtcNow;
                                t.LocalStatus = LocalStatus.DELETED;
                                t.ToBeSynced = true;
                            });

                            await subTasks.ForEachAsync(t =>
                            {
                                t.UpdatedAt = DateTimeOffset.UtcNow;
                                t.LocalStatus = LocalStatus.DELETED;
                                t.ToBeSynced = true;
                            });

                            context.UpdateRange(tasksToUpdate);
                            if (subTasks.Count() > 0)
                                context.UpdateRange(subTasks);
                        }
                        response.Succeed = await context.SaveChangesAsync() > 0;
                        _logger.Information("RemoveTaskAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "RemoveTaskAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<GoogleTask>> ChangeTaskStatusAsync(string taskID, GoogleTaskStatus taskStatus)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"ChangeTaskStatusAsync: Trying to change the status of taskID = {taskID} to {taskStatus}");
                var response = new ResponseDto<GoogleTask>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        var taskToUpdate = await context
                            .Tasks
                            .FirstOrDefaultAsync(t => t.GoogleTaskID == taskID);

                        if (taskToUpdate == null)
                        {
                            response.Message = $"Could not find the task with taskID = {taskID} to change their status";
                            _logger.Warning($"ChangeTaskStatusAsync: Could not find a task with taskID = {taskID}");
                            return response;
                        }

                        taskToUpdate.CompletedOn = taskStatus == GoogleTaskStatus.COMPLETED ?
                            DateTimeOffset.UtcNow : (DateTimeOffset?)null;
                        taskToUpdate.Status = taskStatus.GetString();
                        taskToUpdate.UpdatedAt = DateTimeOffset.UtcNow;
                        if (taskToUpdate.LocalStatus != LocalStatus.CREATED)
                            taskToUpdate.LocalStatus = LocalStatus.UPDATED;
                        taskToUpdate.ToBeSynced = true;

                        context.Tasks.Update(taskToUpdate);

                        response.Succeed = await context.SaveChangesAsync() > 0;
                        response.Result = taskToUpdate;
                        _logger.Information("ChangeTaskStatusAsync: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "ChangeTaskStatusAsync: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<ResponseDto<GoogleTask>> RemoveNotificationDate(string taskID, TaskNotificationDateType dateType)
        {
            return await Task.Run(async () =>
            {
                string dateToRemoveType = dateType == TaskNotificationDateType.REMINDER_DATE ?
                    "reminder" : "completition";
                _logger.Information($"RemoveDate: Trying to remove the {dateToRemoveType} date of taskID = {taskID}");
                var response = new ResponseDto<GoogleTask>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        var taskToUpdate = await context
                            .Tasks
                            .FirstOrDefaultAsync(t => t.GoogleTaskID == taskID);

                        if (taskToUpdate == null)
                        {
                            response.Message = $"Could not find the task with taskID = {taskID} to remove its {dateToRemoveType} date";
                            _logger.Warning($"RemoveDate: Could not find a task with taskID = {taskID}");
                            return response;
                        }

                        switch (dateType)
                        {
                            case TaskNotificationDateType.TO_BE_COMPLETED_DATE:
                                taskToUpdate.ToBeCompletedOn = null;
                                break;
                            case TaskNotificationDateType.REMINDER_DATE:
                                taskToUpdate.RemindOn = null;
                                taskToUpdate.RemindOnGUID = null;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(dateType), dateType, "Provided google task date type does not exists");
                        }

                        context.Tasks.Update(taskToUpdate);

                        response.Succeed = await context.SaveChangesAsync() > 0;
                        response.Result = taskToUpdate;
                        _logger.Information("RemoveDate: Completed successfully");
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "RemoveDate: An unknown error occurred");
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        private string GetExceptionMessage(Exception e)
        {
            string result = string.Empty;
            string inner = e.InnerException?.Message;
            if (!string.IsNullOrEmpty(inner))
                result = $"{e.Message}. Inner Exception: {inner}";
            else
                result = $"{e.Message}. StackTrace: {e.StackTrace}";
            return result;
        }
    }
}
