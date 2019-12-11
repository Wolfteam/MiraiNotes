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
    public class TaskListDataService : BaseDataService, ITaskListDataService
    {
        private readonly ILogger _logger;

        public TaskListDataService(ILogger logger, ITelemetryService telemetryService)
            : base(telemetryService)
        {
            _logger = logger.ForContext<TaskListDataService>();
        }

        public async Task<ResponseDto<GoogleTaskList>> AddAsync(GoogleTaskList entity)
        {
            return await Task.Run(async () =>
            {
                _logger.Information("AddAsync: Trying to add a new task list");
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

        public async Task<ResponseDto<GoogleTaskList>> FirstOrDefaultAsNoTrackingAsync(Expression<Func<GoogleTaskList, bool>> predicate)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"FirstOrDefaultAsNoTrackingAsync: Trying to find the first task list that matches predicate");
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

        public async Task<EmptyResponseDto> RemoveAsync(GoogleTaskList entity)
        {
            return await Task.Run(async () =>
            {
                _logger.Information($"RemoveAsync: Trying to delete taskId = {entity.ID}");
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
                _logger.Information($"UpdateAsync: Trying to update {entity.ID}");
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
    }
}
