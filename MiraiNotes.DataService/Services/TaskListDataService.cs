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
    public class TaskListDataService : ITaskListDataService
    {
        public async Task<Result> AddAsync(GoogleTaskList entity)
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
                    var currentUser = await context.Users
                       .FirstOrDefaultAsync(u => u.IsActive);

                    if (currentUser == null)
                    {
                        result.Message = "Couldn't find the current active user in the db";
                    }
                    else
                    {
                        entity.User = currentUser;
                        await context.AddAsync(entity);
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

        public async Task<Result> AddRangeAsync(IEnumerable<GoogleTaskList> entities)
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
                    var currentUser = await context.Users
                        .FirstOrDefaultAsync(u => u.IsActive);

                    if (currentUser == null)
                    {
                        result.Message = "Couldn't find the current active user in the db";
                    }
                    else
                    {
                        var e = entities.ToList();
                        e.ForEach(tl => tl.User = currentUser);

                        await context.AddRangeAsync(e);
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

        public async Task<bool> ExistsAsync(Expression<Func<GoogleTaskList, bool>> predicate)
        {
            bool result = false;
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    result = await context.TaskLists
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

        public async Task<GoogleTaskList> FirstOrDefaultAsync(Expression<Func<GoogleTaskList, bool>> predicate)
        {
            try
            {
                using (var context = new MiraiNotesContext())
                {
                    return await context
                        .TaskLists
                        .FirstOrDefaultAsync(predicate);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<GoogleTaskList> FirstOrDefaultAsNoTrackingAsync(Expression<Func<GoogleTaskList, bool>> predicate)
        {
            try
            {
                using (var context = new MiraiNotesContext())
                {
                    return await context
                        .TaskLists
                        .AsNoTracking()
                        .FirstOrDefaultAsync(predicate);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<GoogleTaskList>> GetAllAsNoTrackingAsync()
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    return await context
                        .TaskLists
                        .AsNoTracking()
                        .ToListAsync();
                }
                catch (Exception)
                {
                    return Enumerable.Empty<GoogleTaskList>();
                }
            }
        }

        public async Task<IEnumerable<GoogleTaskList>> GetAllAsync()
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    return await context
                        .TaskLists
                        .ToListAsync();
                }
                catch (Exception)
                {
                    return Enumerable.Empty<GoogleTaskList>();
                }
            }
        }

        public async Task<IEnumerable<GoogleTaskList>> GetAsNoTrackingAsync(Expression<Func<GoogleTaskList, bool>> filter = null, Func<IQueryable<GoogleTaskList>, IOrderedQueryable<GoogleTaskList>> orderBy = null, string includeProperties = "")
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    IQueryable<GoogleTaskList> query = context.TaskLists;
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
                    return Enumerable.Empty<GoogleTaskList>();
                }
            }
        }

        public async Task<IEnumerable<GoogleTaskList>> GetAsync(Expression<Func<GoogleTaskList, bool>> predicate)
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    return await context
                        .TaskLists
                        .Where(predicate)
                        .ToListAsync();
                }
                catch (Exception)
                {
                    return Enumerable.Empty<GoogleTaskList>();
                }
            }
        }

        public async Task<GoogleTaskList> GetAsync(Expression<Func<GoogleTaskList, bool>> filter, string includeProperties = "")
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    IQueryable<GoogleTaskList> query = context.TaskLists;
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

        public async Task<IEnumerable<GoogleTaskList>> GetAsync(Expression<Func<GoogleTaskList, bool>> filter = null, Func<IQueryable<GoogleTaskList>, IOrderedQueryable<GoogleTaskList>> orderBy = null, string includeProperties = "")
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    IQueryable<GoogleTaskList> query = context.TaskLists;
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
                    return Enumerable.Empty<GoogleTaskList>();
                }
            }
        }

        public async Task<GoogleTaskList> GetByIdAsync(object id)
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    return await context
                        .TaskLists
                        .FindAsync(id);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public async Task<Result> RemoveAsync(GoogleTaskList entity)
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

        public async Task<Result> RemoveAsync(Expression<Func<GoogleTaskList, bool>> filter = null)
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

        public async Task<Result> RemoveRangeAsync(IEnumerable<GoogleTaskList> entities)
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

        public async Task<Result> UpdateAsync(GoogleTaskList entity)
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

        public Task<Result> UpdateRangeAsync(IEnumerable<GoogleTaskList> entities)
        {
            throw new NotImplementedException();
        }
    }
}
