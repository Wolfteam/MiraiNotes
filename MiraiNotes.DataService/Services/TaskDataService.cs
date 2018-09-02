using Microsoft.EntityFrameworkCore;
using MiraiNotes.Data;
using MiraiNotes.Data.Models;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MiraiNotes.DataService.Services
{
    public class TaskDataService : ITaskDataService
    {
        public async Task<Response<GoogleTask>> AddAsync(GoogleTask entity)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleTask>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<GoogleTask>> AddAsync(string taskListID, GoogleTask task)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleTask>
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
                        }
                        else
                        {
                            task.TaskList = taskList;
                            await context.AddAsync(task);
                            response.Succeed = await context.SaveChangesAsync() > 0;
                            response.Result = task;
                        }
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<IEnumerable<GoogleTask>>> AddRangeAsync(IEnumerable<GoogleTask> entities)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTask>>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<IEnumerable<GoogleTask>>> AddRangeAsync(string taskListID, IEnumerable<GoogleTask> tasks)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTask>>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<bool>> ExistsAsync(Expression<Func<GoogleTask, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<bool>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<GoogleTask>> FirstOrDefaultAsync(Expression<Func<GoogleTask, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleTask>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<GoogleTask>> FirstOrDefaultAsNoTrackingAsync(Expression<Func<GoogleTask, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleTask>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<IEnumerable<GoogleTask>>> GetAllAsNoTrackingAsync()
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTask>>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<IEnumerable<GoogleTask>>> GetAllAsync()
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTask>>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<IEnumerable<GoogleTask>>> GetAsNoTrackingAsync(Expression<Func<GoogleTask, bool>> filter = null, Func<IQueryable<GoogleTask>, IOrderedQueryable<GoogleTask>> orderBy = null, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTask>>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<IEnumerable<GoogleTask>>> GetAsync(Expression<Func<GoogleTask, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTask>>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<GoogleTask>> GetAsync(Expression<Func<GoogleTask, bool>> filter, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleTask>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<IEnumerable<GoogleTask>>> GetAsync(Expression<Func<GoogleTask, bool>> filter = null, Func<IQueryable<GoogleTask>, IOrderedQueryable<GoogleTask>> orderBy = null, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTask>>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<GoogleTask>> GetByIdAsync(object id)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleTask>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponse> RemoveAsync(GoogleTask entity)
        {
            return await Task.Run(async () =>
            {
                var response = new EmptyResponse
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponse> RemoveAsync(object id)
        {
            return await Task.Run(async () =>
            {
                var response = new EmptyResponse
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        var entity = await GetByIdAsync(id);
                        if (entity == null)
                        {
                            response.Message = "Entity couldn't be removed cause it wasnt found";
                        }
                        else
                        {
                            context.Remove(entity);
                            response.Succeed = await context.SaveChangesAsync() > 0;
                        }
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponse> RemoveAsync(Expression<Func<GoogleTask, bool>> filter = null)
        {
            return await Task.Run(async () =>
            {
                var response = new EmptyResponse
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        var entities = await GetAsync(filter);
                        context.RemoveRange(entities);
                        response.Succeed = await context.SaveChangesAsync() > 0;
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponse> RemoveRangeAsync(IEnumerable<GoogleTask> entities)
        {
            return await Task.Run(async () =>
            {
                var response = new EmptyResponse
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<GoogleTask>> UpdateAsync(GoogleTask entity)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleTask>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<IEnumerable<GoogleTask>>> UpdateRangeAsync(IEnumerable<GoogleTask> entities)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTask>>
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
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<GoogleTask>> MoveAsync(string selectedTaskListID, string taskID, string parentTask, string previous)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleTask>
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
                            return response;
                        }

                        var oldEntity = await context
                            .Tasks
                            .FirstOrDefaultAsync(t => t.GoogleTaskID == taskID);

                        if (oldEntity == null)
                        {
                            response.Message = $"Couldn't find the task to be moved in db";
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
                            CreatedAt = DateTime.Now,
                            GoogleTaskID = Guid.NewGuid().ToString(),
                            IsDeleted = oldEntity.IsDeleted,
                            IsHidden = oldEntity.IsHidden,
                            LocalStatus = LocalStatus.CREATED,
                            Notes = oldEntity.Notes,
                            ParentTask = parentTask,
                            Position = previous,
                            Status = oldEntity.Status,
                            TaskList = taskList,
                            Title = oldEntity.Title,
                            ToBeCompletedOn = oldEntity.ToBeCompletedOn,
                            ToBeSynced = true,
                            UpdatedAt = DateTime.Now
                        };

                        if (oldEntity.LocalStatus == LocalStatus.CREATED)
                            context.Remove(oldEntity);
                        else
                        {
                            oldEntity.LocalStatus = LocalStatus.DELETED;
                            oldEntity.ToBeSynced = true;
                            oldEntity.UpdatedAt = DateTime.Now;
                        }

                        await context.Tasks.AddAsync(entity);

                        response.Succeed = await context.SaveChangesAsync() > 0;
                        response.Result = entity;
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponse> RemoveTaskAsync(string taskID)
        {
            return await Task.Run(async () =>
            {
                var response = new EmptyResponse
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
                            entity.UpdatedAt = DateTime.Now;
                            entity.ToBeSynced = true;

                            context.Update(entity);

                            if (subTasks.Count() > 0)
                            {
                                await subTasks.ForEachAsync(st =>
                                {
                                    st.LocalStatus = LocalStatus.DELETED;
                                    st.UpdatedAt = DateTime.Now;
                                    st.ToBeSynced = true;
                                });
                                context.UpdateRange(subTasks);
                            }
                        }
                        response.Succeed = await context.SaveChangesAsync() > 0;
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponse> RemoveTaskAsync(IEnumerable<string> taskIds)
        {
            return await Task.Run(async () =>
            {
                var response = new EmptyResponse
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
                                t.UpdatedAt = DateTime.Now;
                                t.LocalStatus = LocalStatus.DELETED;
                                t.ToBeSynced = true;
                            });

                            await subTasks.ForEachAsync(t =>
                            {
                                t.UpdatedAt = DateTime.Now;
                                t.LocalStatus = LocalStatus.DELETED;
                                t.ToBeSynced = true;
                            });

                            context.UpdateRange(tasksToUpdate);
                            if (subTasks.Count() > 0)
                                context.UpdateRange(subTasks);
                        }
                        response.Succeed = await context.SaveChangesAsync() > 0;
                    }
                    catch (Exception e)
                    {
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
