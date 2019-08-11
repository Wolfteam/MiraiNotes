using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;
using Serilog;

namespace MiraiNotes.Shared.Services.Data
{
    public class TaskListDataService : ITaskListDataService
    {
        private readonly ILogger _logger;
        public TaskListDataService(ILogger logger)
        {
            _logger = logger.ForContext<TaskListDataService>();
        }

        public async Task<ResponseDto<GoogleTaskList>> AddAsync(GoogleTaskList entity)
        {
            return await Task.Run(async () =>
            {
                _logger.Information("AddAsync: Trying to add a new task list {@TaskList}", entity);
                var response = new ResponseDto<GoogleTaskList>
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
                            _logger.Warning("AddAsync: Couldn't find the current active user in the db");
                        }
                        else
                        {
                            entity.User = currentUser;
                            await context.AddAsync(entity);
                            response.Succeed = await context.SaveChangesAsync() > 0;
                            response.Result = entity;
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

        public async Task<ResponseDto<IEnumerable<GoogleTaskList>>> AddRangeAsync(IEnumerable<GoogleTaskList> entities)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"AddRangeAsync: Trying to add {entities.Count()} task lists");
                var response = new ResponseDto<IEnumerable<GoogleTaskList>>
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
                            _logger.Warning("AddRangeAsync: Couldn't find the current active user in the db");
                        }
                        else
                        {
                            var e = entities.ToList();
                            e.ForEach(tl => tl.User = currentUser);

                            await context.AddRangeAsync(e);
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

        public async Task<ResponseDto<bool>> ExistsAsync(Expression<Func<GoogleTaskList, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"ExistsAsync: Trying to find the first task list that matches {predicate.ToString()}");
                var response = new ResponseDto<bool>
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

        public async Task<ResponseDto<GoogleTaskList>> FirstOrDefaultAsync(Expression<Func<GoogleTaskList, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"FirstOrDefaultAsync: Trying to find the first task list that matches {predicate.ToString()}");
                var response = new ResponseDto<GoogleTaskList>
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

        public async Task<ResponseDto<GoogleTaskList>> FirstOrDefaultAsNoTrackingAsync(Expression<Func<GoogleTaskList, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"FirstOrDefaultAsNoTrackingAsync: Trying to find the first task list that matches {predicate.ToString()}");
                var response = new ResponseDto<GoogleTaskList>
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

        public async Task<ResponseDto<IEnumerable<GoogleTaskList>>> GetAllAsNoTrackingAsync()
        {
            return await Task.Run(async () =>
            {
                _logger.Information("GetAllAsNoTrackingAsync: Trying to get all the task list");
                var response = new ResponseDto<IEnumerable<GoogleTaskList>>
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

        public async Task<ResponseDto<IEnumerable<GoogleTaskList>>> GetAllAsync()
        {
            return await Task.Run(async () =>
            {
                _logger.Information("GetAllAsync: Trying to get all the task lists");
                var response = new ResponseDto<IEnumerable<GoogleTaskList>>
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

        public async Task<ResponseDto<IEnumerable<GoogleTaskList>>> GetAsNoTrackingAsync(Expression<Func<GoogleTaskList, bool>> filter = null, Func<IQueryable<GoogleTaskList>, IOrderedQueryable<GoogleTaskList>> orderBy = null, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                _logger.Information("GetAsNoTrackingAsync: Getting all task lists");
                var response = new ResponseDto<IEnumerable<GoogleTaskList>>
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

        public async Task<ResponseDto<IEnumerable<GoogleTaskList>>> GetAsync(Expression<Func<GoogleTaskList, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"GetAsync: Getting all task lists that matches {predicate.ToString()}");
                var response = new ResponseDto<IEnumerable<GoogleTaskList>>
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

        public async Task<ResponseDto<GoogleTaskList>> GetAsync(Expression<Func<GoogleTaskList, bool>> filter, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"GetAsync: Getting all task lists with {filter.ToString()}");
                var response = new ResponseDto<GoogleTaskList>
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

        public async Task<ResponseDto<IEnumerable<GoogleTaskList>>> GetAsync(Expression<Func<GoogleTaskList, bool>> filter = null, Func<IQueryable<GoogleTaskList>, IOrderedQueryable<GoogleTaskList>> orderBy = null, string includeProperties = "")
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"GetAsync: Getting all task lists with {filter?.ToString() ?? string.Empty}");
                var response = new ResponseDto<IEnumerable<GoogleTaskList>>
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

        public async Task<ResponseDto<GoogleTaskList>> GetByIdAsync(object id)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"GetByIdAsync: Looking for task list with id = {id}");
                var response = new ResponseDto<GoogleTaskList>
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

        public async Task<EmptyResponseDto> RemoveAsync(GoogleTaskList entity)
        {
            return await Task.Run(async () =>
            {
                _logger.Information("RemoveAsync: Trying to delete {@TaskList}", entity);
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
                _logger.Information($"RemoveAsync: Trying to delete task list with id {id}");
                var response = new EmptyResponseDto
                {
                    Message = string.Empty,
                    Succeed = false
                };
                using (var context = new MiraiNotesContext())
                {
                    try
                    {
                        var entity = await context.TaskLists.FindAsync(id);
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

        public async Task<EmptyResponseDto> RemoveAsync(Expression<Func<GoogleTaskList, bool>> filter)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"RemoveAsync: Trying to delete task lists that matches {filter.ToString()}");
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
                            .TaskLists
                            .Where(filter);

                        if (entities.Count() > 0)
                        {
                            context.RemoveRange(entities);
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

        public async Task<EmptyResponseDto> RemoveRangeAsync(IEnumerable<GoogleTaskList> entities)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"RemoveRangeAsync: Trying to delete {entities.Count()} task lists");
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

        public async Task<ResponseDto<GoogleTaskList>> UpdateAsync(GoogleTaskList entity)
        {
            return await Task.Run(async () =>
            {
                _logger.Information("UpdateAsync: Trying to update {@TaskList}", entity);
                var response = new ResponseDto<GoogleTaskList>
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

        public async Task<ResponseDto<IEnumerable<GoogleTaskList>>> UpdateRangeAsync(IEnumerable<GoogleTaskList> entities)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"UpdateRangeAsync: Trying to update {entities.Count()} task lists");
                var response = new ResponseDto<IEnumerable<GoogleTaskList>>
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
