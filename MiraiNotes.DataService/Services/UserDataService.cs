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
        public async Task<GoogleUser> GetCurrentActiveUserAsync()
        {
            var currentUser = await GetAsync(u => u.IsActive, null, string.Empty);
            if (currentUser.Count() > 1)
            {
                throw new ApplicationException($"We cant have more than 1 active user. Current active users = {currentUser.Count()}");
            }
            var user = currentUser.FirstOrDefault();
            return user;
        }

        public async Task ChangeCurrentUserStatus(bool isActive)
        {
            var currentUser = await GetCurrentActiveUserAsync();
            if (currentUser == null)
                throw new NullReferenceException("The current active user couldnt be found on db");
            currentUser.IsActive = isActive;
//todo retun something here
            await UpdateAsync(currentUser);
        }

        public async Task<Result> AddAsync(GoogleUser entity)
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

        public async Task<Result> AddRangeAsync(IEnumerable<GoogleUser> entities)
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

        public async Task<bool> ExistsAsync(Expression<Func<GoogleUser, bool>> predicate)
        {
            bool result = false;
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    result = await context.Users
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

        public async Task<GoogleUser> FirstOrDefaultAsync(Expression<Func<GoogleUser, bool>> predicate)
        {
            try
            {
                using (var context = new MiraiNotesContext())
                {
                    return await context
                        .Users
                        .FirstOrDefaultAsync(predicate);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<GoogleUser> FirstOrDefaultAsNoTrackingAsync(Expression<Func<GoogleUser, bool>> predicate)
        {
            try
            {
                using (var context = new MiraiNotesContext())
                {
                    return await context
                        .Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(predicate);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<GoogleUser>> GetAllAsNoTrackingAsync()
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    return await context
                        .Users
                        .AsNoTracking()
                        .ToListAsync();
                }
                catch (Exception)
                {
                    return Enumerable.Empty<GoogleUser>();
                }
            }
        }

        public async Task<IEnumerable<GoogleUser>> GetAllAsync()
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    return await context
                        .Users
                        .ToListAsync();
                }
                catch (Exception)
                {
                    return Enumerable.Empty<GoogleUser>();
                }
            }
        }

        public async Task<IEnumerable<GoogleUser>> GetAsNoTrackingAsync(Expression<Func<GoogleUser, bool>> filter = null, Func<IQueryable<GoogleUser>, IOrderedQueryable<GoogleUser>> orderBy = null, string includeProperties = "")
        {
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
                    return Enumerable.Empty<GoogleUser>();
                }
            }
        }

        public async Task<IEnumerable<GoogleUser>> GetAsync(Expression<Func<GoogleUser, bool>> predicate)
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    return await context
                        .Users
                        .Where(predicate)
                        .ToListAsync();
                }
                catch (Exception)
                {
                    return Enumerable.Empty<GoogleUser>();
                }
            }
        }

        public async Task<GoogleUser> GetAsync(Expression<Func<GoogleUser, bool>> filter, string includeProperties = "")
        {
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

                    return await query
                        .FirstOrDefaultAsync();

                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public async Task<IEnumerable<GoogleUser>> GetAsync(Expression<Func<GoogleUser, bool>> filter = null, Func<IQueryable<GoogleUser>, IOrderedQueryable<GoogleUser>> orderBy = null, string includeProperties = "")
        {
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
                        return await orderBy(query).ToListAsync();
                    }
                    else
                    {
                        return await query.ToListAsync();
                    }
                }
                catch (Exception)
                {
                    return Enumerable.Empty<GoogleUser>();
                }
            }
        }

        public async Task<GoogleUser> GetByIdAsync(object id)
        {
            using (var context = new MiraiNotesContext())
            {
                try
                {
                    return await context
                        .Users
                        .FindAsync(id);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public async Task<Result> RemoveAsync(GoogleUser entity)
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

        public async Task<Result> RemoveAsync(Expression<Func<GoogleUser, bool>> filter = null)
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

        public async Task<Result> RemoveRangeAsync(IEnumerable<GoogleUser> entities)
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

        public async Task<Result> UpdateAsync(GoogleUser entity)
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

        public Task<Result> UpdateRangeAsync(IEnumerable<GoogleUser> entities)
        {
            throw new NotImplementedException();
        }
    }
}
