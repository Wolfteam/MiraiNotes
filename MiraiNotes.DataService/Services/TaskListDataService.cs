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
        public async Task<Response<GoogleTaskList>> AddAsync(GoogleTaskList entity)
        {
            var response = new Response<GoogleTaskList>
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
                        response.Message = "Couldn't find the current active user in the db";
                    }
                    else
                    {
                        entity.User = currentUser;
                        await context.AddAsync(entity);
                        response.Succeed = await context.SaveChangesAsync() > 0;
                        response.Result = entity;
                    }
                }
                catch (Exception e)
                {
                    response.Message = GetExceptionMessage(e);
                }
            }
            return response;
        }

        public async Task<Response<IEnumerable<GoogleTaskList>>> AddRangeAsync(IEnumerable<GoogleTaskList> entities)
        {
            var response = new Response<IEnumerable<GoogleTaskList>>
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
                        response.Message = "Couldn't find the current active user in the db";
                    }
                    else
                    {
                        var e = entities.ToList();
                        e.ForEach(tl => tl.User = currentUser);

                        await context.AddRangeAsync(e);
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
        }

        public async Task<Response<bool>> ExistsAsync(Expression<Func<GoogleTaskList, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<bool>
                {
                    Succeed = false,
                    Message = string.Empty
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context.TaskLists
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

        public async Task<Response<GoogleTaskList>> FirstOrDefaultAsync(Expression<Func<GoogleTaskList, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleTaskList>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {

                        response.Result = await context
                            .TaskLists
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

        public async Task<Response<GoogleTaskList>> FirstOrDefaultAsNoTrackingAsync(Expression<Func<GoogleTaskList, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleTaskList>
                {
                    Message = string.Empty,
                    Succeed = false
                };

                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .TaskLists
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

        public async Task<Response<IEnumerable<GoogleTaskList>>> GetAllAsNoTrackingAsync()
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTaskList>>
                {
                    Message = string.Empty,
                    Succeed = false
                };

                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .TaskLists
                            .AsNoTracking()
                            .ToListAsync();
                        response.Succeed = true;
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                        response.Result = Enumerable.Empty<GoogleTaskList>();
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<IEnumerable<GoogleTaskList>>> GetAllAsync()
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTaskList>>
                {
                    Message = string.Empty,
                    Succeed = false
                };

                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .TaskLists
                            .ToListAsync();
                        response.Succeed = true;
                    }
                    catch (Exception e)
                    {
                        response.Result = Enumerable.Empty<GoogleTaskList>();
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<IEnumerable<GoogleTaskList>>> GetAsNoTrackingAsync(Expression<Func<GoogleTaskList, bool>> filter = null, Func<IQueryable<GoogleTaskList>, IOrderedQueryable<GoogleTaskList>> orderBy = null, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTaskList>>
                {
                    Message = string.Empty,
                    Succeed = false
                };

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
                        response.Result = Enumerable.Empty<GoogleTaskList>();
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<IEnumerable<GoogleTaskList>>> GetAsync(Expression<Func<GoogleTaskList, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTaskList>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .TaskLists
                            .Where(predicate)
                            .ToListAsync();
                        response.Succeed = true;
                    }
                    catch (Exception e)
                    {
                        response.Result = Enumerable.Empty<GoogleTaskList>();
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<GoogleTaskList>> GetAsync(Expression<Func<GoogleTaskList, bool>> filter, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleTaskList>
                {
                    Message = string.Empty,
                    Succeed = false
                };
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

        public async Task<Response<IEnumerable<GoogleTaskList>>> GetAsync(Expression<Func<GoogleTaskList, bool>> filter = null, Func<IQueryable<GoogleTaskList>, IOrderedQueryable<GoogleTaskList>> orderBy = null, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTaskList>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
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
                        response.Result = Enumerable.Empty<GoogleTaskList>();
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<GoogleTaskList>> GetByIdAsync(object id)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleTaskList>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .TaskLists
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

        public async Task<EmptyResponse> RemoveAsync(GoogleTaskList entity)
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

        public async Task<EmptyResponse> RemoveAsync(Expression<Func<GoogleTaskList, bool>> filter = null)
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

        public async Task<EmptyResponse> RemoveRangeAsync(IEnumerable<GoogleTaskList> entities)
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

        public async Task<Response<GoogleTaskList>> UpdateAsync(GoogleTaskList entity)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleTaskList>
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

        public async Task<Response<IEnumerable<GoogleTaskList>>> UpdateRangeAsync(IEnumerable<GoogleTaskList> entities)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleTaskList>>
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
