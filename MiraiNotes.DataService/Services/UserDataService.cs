using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiraiNotes.Data;
using MiraiNotes.Data.Models;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.Shared.Models;

namespace MiraiNotes.DataService.Services
{
    public class UserDataService : IUserDataService
    {
        public async Task<Response<GoogleUser>> GetCurrentActiveUserAsync()
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleUser>
                {
                    Succeed = false,
                    Message = string.Empty
                };

                var currentUserResponse = await GetAsync(u => u.IsActive, null, string.Empty);
                if (!currentUserResponse.Succeed)
                    response.Message = currentUserResponse.Message;
                else if (currentUserResponse.Result.Count() > 1)
                    response.Message = $"We cant have more than 1 active user. Current active users = {currentUserResponse.Result.Count()}";
                else
                {
                    response.Result = currentUserResponse.Result.FirstOrDefault();
                    response.Succeed = true;
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponse> ChangeCurrentUserStatus(bool isActive)
        {
            return await Task.Run(async () =>
            {
                var response = new EmptyResponse
                {
                    Succeed = false,
                    Message = string.Empty
                };

                var currentUserResponse = await GetCurrentActiveUserAsync();
                if (!currentUserResponse.Succeed)
                    return currentUserResponse;

                currentUserResponse.Result.IsActive = isActive;
                return await UpdateAsync(currentUserResponse.Result);
            }).ConfigureAwait(false);
        }

        public async Task<Response<GoogleUser>> AddAsync(GoogleUser entity)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleUser>
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

        public async Task<Response<IEnumerable<GoogleUser>>> AddRangeAsync(IEnumerable<GoogleUser> entities)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleUser>>
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

        public async Task<Response<bool>> ExistsAsync(Expression<Func<GoogleUser, bool>> predicate)
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
                        response.Result = await context.Users
                            .AsNoTracking()
                            .Where(predicate)
                            .CountAsync() == 1;
                    }
                    catch (Exception e)
                    {
                        response.Message = GetExceptionMessage(e);
                    }
                }
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<Response<GoogleUser>> FirstOrDefaultAsync(Expression<Func<GoogleUser, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleUser>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .Users
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

        public async Task<Response<GoogleUser>> FirstOrDefaultAsNoTrackingAsync(Expression<Func<GoogleUser, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleUser>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .Users
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

        public async Task<Response<IEnumerable<GoogleUser>>> GetAllAsNoTrackingAsync()
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleUser>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result =  await context
                            .Users
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

        public async Task<Response<IEnumerable<GoogleUser>>> GetAllAsync()
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleUser>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .Users
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

        public async Task<Response<IEnumerable<GoogleUser>>> GetAsNoTrackingAsync(Expression<Func<GoogleUser, bool>> filter = null, Func<IQueryable<GoogleUser>, IOrderedQueryable<GoogleUser>> orderBy = null, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleUser>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        IQueryable<GoogleUser> query = context.Users;
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

        public async Task<Response<IEnumerable<GoogleUser>>> GetAsync(Expression<Func<GoogleUser, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleUser>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .Users
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

        public async Task<Response<GoogleUser>> GetAsync(Expression<Func<GoogleUser, bool>> filter, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleUser>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        IQueryable<GoogleUser> query = context.Users;
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

        public async Task<Response<IEnumerable<GoogleUser>>> GetAsync(Expression<Func<GoogleUser, bool>> filter = null, Func<IQueryable<GoogleUser>, IOrderedQueryable<GoogleUser>> orderBy = null, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                var response = new Response<IEnumerable<GoogleUser>>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        IQueryable<GoogleUser> query = context.Users;
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

        public async Task<Response<GoogleUser>> GetByIdAsync(object id)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleUser>
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        response.Result = await context
                            .Users
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

        public async Task<EmptyResponse> RemoveAsync(GoogleUser entity)
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
                            response.Message = "Entity couldn't be removed cause it wasnt found";
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

        public async Task<EmptyResponse> RemoveAsync(Expression<Func<GoogleUser, bool>> filter = null)
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

        public async Task<EmptyResponse> RemoveRangeAsync(IEnumerable<GoogleUser> entities)
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

        public async Task<Response<GoogleUser>> UpdateAsync(GoogleUser entity)
        {
            return await Task.Run(async () =>
            {
                var response = new Response<GoogleUser>
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

        public Task<Response<IEnumerable<GoogleUser>>> UpdateRangeAsync(IEnumerable<GoogleUser> entities)
        {
            throw new NotImplementedException();
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
