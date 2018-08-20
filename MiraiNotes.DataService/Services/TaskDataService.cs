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

        //public async Task<Result> MoveAsync(string selectedTaskListID, string taskID, string position)
        //{
        //    var result = new Result
        //    {
        //        Message = string.Empty,
        //        Succeed = false
        //    };
        //    var taskList = await MiraiNotesContext
        //        .TaskLists
        //        .FirstOrDefaultAsync(tl => tl.GoogleTaskListID == selectedTaskListID);

        //    if (taskList == null)
        //    {
        //        result.Message = $"Couldn't find a tasklist with id = {selectedTaskListID} in the db";
        //        return result;
        //    }

        //    var oldEntity = await
        //        base.FirstOrDefaultAsync(t => t.GoogleTaskID == taskID);

        //    if (oldEntity == null)
        //    {
        //        result.Message = $"Couldn't find a task with id = {taskID} in the db";
        //        return result;
        //    }

        //    oldEntity.LocalStatus = LocalStatus.DELETED;
        //    oldEntity.ToBeSynced = true;
        //    oldEntity.UpdatedAt = DateTime.Now;

        //    var entity = new GoogleTask
        //    {
        //        CompletedOn = oldEntity.CompletedOn,
        //        CreatedAt = DateTime.Now,
        //        GoogleTaskID = oldEntity.GoogleTaskID,
        //        IsDeleted = oldEntity.IsDeleted,
        //        IsHidden = oldEntity.IsHidden,
        //        LocalStatus = LocalStatus.CREATED,
        //        Notes = oldEntity.Notes,
        //        ParentTask = oldEntity.ParentTask,
        //        Position = position,
        //        Status = oldEntity.Status,
        //        TaskList = taskList,
        //        Title = oldEntity.Title,
        //        ToBeCompletedOn = oldEntity.ToBeCompletedOn,
        //        ToBeSynced = true,
        //        UpdatedAt = DateTime.Now
        //    };

        //    await base.AddAsync(entity);

        //    result.Succeed = true;
        //    return result;
        //}
        public async Task<Result> AddAsync(GoogleTask entity)
        {
            var result = new Result
            {
                Message = string.Empty,
                Succeed = false
            };
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    await context.AddAsync(entity);
                    result.Succeed = await context.SaveChangesAsync() > 0;
                }
                catch (Exception e)
                {
                    string inner = e.InnerException?.Message;
                    if (!string.IsNullOrEmpty(inner))
                        result.Message = $"{e.Message}. Inner Exception: {inner}";
                    else
                        result.Message = $"{e.Message}. StackTrace: {e.StackTrace}";
                }
            }
            return result;
        }

        public async Task<Result> AddAsync(string taskListID, GoogleTask task)
        {
            var result = new Result
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
                        result.Message = $"Couldn't find the tasklist where {task.Title} is going to be saved";
                    }
                    else
                    {
                        task.TaskList = taskList;
                        await context.AddAsync(task);
                        result.Succeed = await context.SaveChangesAsync() > 0;
                    }
                }
                catch (Exception e)
                {
                    string inner = e.InnerException?.Message;
                    if (!string.IsNullOrEmpty(inner))
                        result.Message = $"{e.Message}. Inner Exception: {inner}";
                    else
                        result.Message = $"{e.Message}. StackTrace: {e.StackTrace}";
                }
            }
            return result;
        }

        public async Task<Result> AddRangeAsync(IEnumerable<GoogleTask> entities)
        {
            var result = new Result
            {
                Message = string.Empty,
                Succeed = false
            };
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    await context.AddRangeAsync(entities);
                    result.Succeed = await context.SaveChangesAsync() > 0;
                }
                catch (Exception e)
                {
                    string inner = e.InnerException?.Message;
                    if (!string.IsNullOrEmpty(inner))
                        result.Message = $"{e.Message}. Inner Exception: {inner}";
                    else
                        result.Message = $"{e.Message}. StackTrace: {e.StackTrace}";
                }
            }
            return result;
        }

        public async Task<Result> AddRangeAsync(string taskListID, IEnumerable<GoogleTask> tasks)
        {
            var result = new Result
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
                        result.Message = $"Couldn't find the tasklist where all the tasks will be saved in the db";
                    }
                    else
                    {
                        var entities = tasks.ToList();
                        entities.ForEach(t => t.TaskList = taskList);

                        await context.AddRangeAsync(entities);
                        result.Succeed = await context.SaveChangesAsync() > 0;
                    }
                    
                }
                catch (Exception e)
                {
                    string inner = e.InnerException?.Message;
                    if (!string.IsNullOrEmpty(inner))
                        result.Message = $"{e.Message}. Inner Exception: {inner}";
                    else
                        result.Message = $"{e.Message}. StackTrace: {e.StackTrace}";
                }
            }
            return result;
        }

        public async Task<bool> ExistsAsync(Expression<Func<GoogleTask, bool>> predicate)
        {
            bool result = false;
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    result = await context.Tasks
                        .AsNoTracking()
                        .Where(predicate)
                        .CountAsync() == 1;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
            return result;
        }

        public async Task<GoogleTask> FirstOrDefaultAsync(Expression<Func<GoogleTask, bool>> predicate)
        {
            try
            {
                using (var context = new MiraiNotesContext())
                {
                    return await context
                        .Tasks
                        .FirstOrDefaultAsync(predicate);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<GoogleTask> FirstOrDefaultAsNoTrackingAsync(Expression<Func<GoogleTask, bool>> predicate)
        {
            try
            {
                using (var context = new MiraiNotesContext())
                {
                    return await context
                        .Tasks
                        .AsNoTracking()
                        .FirstOrDefaultAsync(predicate);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<GoogleTask>> GetAllAsNoTrackingAsync()
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    return await context
                        .Tasks
                        .AsNoTracking()
                        .ToListAsync();
                }
                catch (Exception)
                {
                    return Enumerable.Empty<GoogleTask>();
                }
            }
        }

        public async Task<IEnumerable<GoogleTask>> GetAllAsync()
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    return await context
                        .Tasks
                        .ToListAsync();
                }
                catch (Exception)
                {
                    return Enumerable.Empty<GoogleTask>();
                }
            }
        }

        public async Task<IEnumerable<GoogleTask>> GetAsNoTrackingAsync(Expression<Func<GoogleTask, bool>> filter = null, Func<IQueryable<GoogleTask>, IOrderedQueryable<GoogleTask>> orderBy = null, string includeProperties = "")
        {
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
                        return await orderBy(query)
                            .AsNoTracking()
                            .ToListAsync();
                    }
                    else
                    {
                        return await query
                            .AsNoTracking()
                            .ToListAsync();
                    }
                }
                catch (Exception)
                {
                    return Enumerable.Empty<GoogleTask>();
                }
            }
        }

        public async Task<IEnumerable<GoogleTask>> GetAsync(Expression<Func<GoogleTask, bool>> predicate)
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    return await context
                        .Tasks
                        .Where(predicate)
                        .ToListAsync();
                }
                catch (Exception)
                {
                    return Enumerable.Empty<GoogleTask>();
                }
            }
        }

        public async Task<GoogleTask> GetAsync(Expression<Func<GoogleTask, bool>> filter, string includeProperties = "")
        {
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

                    return await query
                        .FirstOrDefaultAsync();

                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public async Task<IEnumerable<GoogleTask>> GetAsync(Expression<Func<GoogleTask, bool>> filter = null, Func<IQueryable<GoogleTask>, IOrderedQueryable<GoogleTask>> orderBy = null, string includeProperties = "")
        {
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
                        return await orderBy(query).ToListAsync();
                    }
                    else
                    {
                        return await query.ToListAsync();
                    }
                }
                catch (Exception)
                {
                    return Enumerable.Empty<GoogleTask>();
                }
            }
        }

        public async Task<GoogleTask> GetByIdAsync(object id)
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    return await context
                        .Tasks
                        .FindAsync(id);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public async Task<Result> RemoveAsync(GoogleTask entity)
        {
            var result = new Result
            {
                Message = string.Empty,
                Succeed = false
            };
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    context.Remove(entity);
                    result.Succeed = await context.SaveChangesAsync() > 0;
                }
                catch (Exception e)
                {
                    string inner = e.InnerException?.Message;
                    if (!string.IsNullOrEmpty(inner))
                        result.Message = $"{e.Message}. Inner Exception: {inner}";
                    else
                        result.Message = $"{e.Message}. StackTrace: {e.StackTrace}";
                }
            }
            return result;
        }

        public async Task<Result> RemoveAsync(object id)
        {
            var result = new Result
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
                        result.Message = "Entity couldn't be removed cause it wasnt found";
                    }
                    else
                    {
                        context.Remove(entity);
                        result.Succeed = await context.SaveChangesAsync() > 0;
                    }
                }
                catch (Exception e)
                {
                    string inner = e.InnerException?.Message;
                    if (!string.IsNullOrEmpty(inner))
                        result.Message = $"{e.Message}. Inner Exception: {inner}";
                    else
                        result.Message = $"{e.Message}. StackTrace: {e.StackTrace}";
                }
            }
            return result;
        }

        public async Task<Result> RemoveAsync(Expression<Func<GoogleTask, bool>> filter = null)
        {
            var result = new Result
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
                    result.Succeed = await context.SaveChangesAsync() > 0;
                }
                catch (Exception e)
                {
                    string inner = e.InnerException?.Message;
                    if (!string.IsNullOrEmpty(inner))
                        result.Message = $"{e.Message}. Inner Exception: {inner}";
                    else
                        result.Message = $"{e.Message}. StackTrace: {e.StackTrace}";
                }
            }
            return result;
        }

        public async Task<Result> RemoveRangeAsync(IEnumerable<GoogleTask> entities)
        {
            var result = new Result
            {
                Message = string.Empty,
                Succeed = false
            };
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    context.RemoveRange(entities);
                    result.Succeed = await context.SaveChangesAsync() > 0;
                }
                catch (Exception e)
                {
                    string inner = e.InnerException?.Message;
                    if (!string.IsNullOrEmpty(inner))
                        result.Message = $"{e.Message}. Inner Exception: {inner}";
                    else
                        result.Message = $"{e.Message}. StackTrace: {e.StackTrace}";
                }
            }
            return result;
        }

        public async Task<Result> UpdateAsync(GoogleTask entity)
        {
            var result = new Result
            {
                Message = string.Empty,
                Succeed = false
            };
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    context.Entry(entity).State = EntityState.Modified;
                    result.Succeed = await context.SaveChangesAsync() > 0;
                }
                catch (Exception e)
                {
                    string inner = e.InnerException?.Message;
                    if (!string.IsNullOrEmpty(inner))
                        result.Message = $"{e.Message}. Inner Exception: {inner}";
                    else
                        result.Message = $"{e.Message}. StackTrace: {e.StackTrace}";
                }
            }
            return result;
        }

        public async Task<Result> UpdateRangeAsync(IEnumerable<GoogleTask> entities)
        {
            var result = new Result
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
                    
                    result.Succeed = await context.SaveChangesAsync() > 0;
                }
                catch (Exception e)
                {
                    string inner = e.InnerException?.Message;
                    if (!string.IsNullOrEmpty(inner))
                        result.Message = $"{e.Message}. Inner Exception: {inner}";
                    else
                        result.Message = $"{e.Message}. StackTrace: {e.StackTrace}";
                }
            }
            return result;
        }
    }
}
