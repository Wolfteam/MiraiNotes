using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;
using Serilog;

namespace MiraiNotes.Shared.Services.Data
{
    public class UserDataService : BaseDataService, IUserDataService
    {
        private readonly ILogger _logger;

        public UserDataService(ILogger logger, ITelemetryService telemetryService)
            : base(telemetryService)
        {
            _logger = logger.ForContext<UserDataService>();
        }

        public async Task<ResponseDto<GoogleUser>> GetCurrentActiveUserAsync()
        {
            return await Task.Run(async () =>
            {
                _logger.Information("GetCurrentActiveUserAsync: Trying to get the current active user");
                var response = new ResponseDto<GoogleUser>
                {
                    Succeed = false,
                    Message = string.Empty
                };

                var currentUserResponse = await GetAsync(u => u.IsActive, null, string.Empty);
                if (!currentUserResponse.Succeed)
                    response.Message = currentUserResponse.Message;
                else if (currentUserResponse.Result.Count() > 1)
                {
                    response.Message = $"We cant have more than 1 active user. Current active users = {currentUserResponse.Result.Count()}";
                    _logger.Warning($"GetCurrentActiveUserAsync: {response.Message}");
                }
                else
                {
                    response.Result = currentUserResponse.Result.FirstOrDefault();
                    response.Succeed = true;
                }
                _logger.Information("GetCurrentActiveUserAsync: Completed successfully");
                return response;
            }).ConfigureAwait(false);
        }

        public async Task<EmptyResponseDto> ChangeCurrentUserStatus(bool isActive)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"ChangeCurrentUserStatus: Trying to change the current active user status to {(isActive ? "Active" : "Inactive")}");
                var response = new EmptyResponseDto
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

        public async Task<ResponseDto<GoogleUser>> AddAsync(GoogleUser entity)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"AddAsync: Trying to add a new user {entity.ID}");
                var response = new ResponseDto<GoogleUser>
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

        public async Task<ResponseDto<bool>> ExistsAsync(Expression<Func<GoogleUser, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"ExistsAsync: Trying to find the first user that matches predicate");
                var response = new ResponseDto<bool>
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

        public async Task<ResponseDto<GoogleUser>> FirstOrDefaultAsNoTrackingAsync(Expression<Func<GoogleUser, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"FirstOrDefaultAsNoTrackingAsync: Trying to find the user that matches predicate");
                var response = new ResponseDto<GoogleUser>
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

        public async Task<ResponseDto<IEnumerable<GoogleUser>>> GetAllAsNoTrackingAsync()
        {
            return await Task.Run(async () =>
            {
                _logger.Information("GetAllAsNoTrackingAsync: Trying to get all users");
                var response = new ResponseDto<IEnumerable<GoogleUser>>
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

        public async Task<ResponseDto<IEnumerable<GoogleUser>>> GetAsNoTrackingAsync(Expression<Func<GoogleUser, bool>> filter = null, Func<IQueryable<GoogleUser>, IOrderedQueryable<GoogleUser>> orderBy = null, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                _logger.Information("GetAsNoTrackingAsync: Getting all users");
                var response = new ResponseDto<IEnumerable<GoogleUser>>
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

        public async Task<ResponseDto<IEnumerable<GoogleUser>>> GetAsync(Expression<Func<GoogleUser, bool>> filter = null, Func<IQueryable<GoogleUser>, IOrderedQueryable<GoogleUser>> orderBy = null, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"GetAsync: Getting all users using a filter");
                var response = new ResponseDto<IEnumerable<GoogleUser>>
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

        public async Task<EmptyResponseDto> RemoveAsync(object id)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"RemoveAsync: Trying to delete user with id {id}");
                var response = new EmptyResponseDto
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        var entity = await context.Users.FindAsync(id);
                        if (entity == null)
                        {
                            response.Message = "Entity couldn't be removed cause it wasnt found";
                            _logger.Warning("RemoveAsync: Task list couldn't be removed cause it wasnt found");
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

        public async Task<ResponseDto<GoogleUser>> UpdateAsync(GoogleUser entity)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"UpdateAsync: Trying to update userId = {entity.ID}");
                var response = new ResponseDto<GoogleUser>
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

        public async Task<EmptyResponseDto> SetAsCurrentUser(string email)
        {
            return await Task.Run(async () =>
            {
                _logger.Information("SetAsCurrentUser: Trying to get the current active user");
                var response = new ResponseDto<GoogleUser>
                {
                    Succeed = false,
                    Message = string.Empty
                };
                using (var context = new MiraiNotesContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.Email == email);
                    if (user is null)
                    {
                        response.Message = $"Email = {email} does not exists in db";
                        _logger.Warning(response.Message);
                        return response;
                    }
                    var activeUser = context.Users.FirstOrDefault(u => u.IsActive);
                    if (activeUser != null)
                    {
                        activeUser.IsActive = false;
                    }
                    user.IsActive = true;
                    response.Succeed = await context.SaveChangesAsync() > 0;
                }
                _logger.Information("SetAsCurrentUser: Completed successfully");
                return response;
            }).ConfigureAwait(false);
        }
    }
}
