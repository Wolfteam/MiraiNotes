using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MiraiNotes.Abstractions.Data
{
    public interface ITaskListDataService
    {
        Task<ResponseDto<GoogleTaskList>> AddAsync(GoogleTaskList entity);

        Task<ResponseDto<IEnumerable<GoogleTaskList>>> AddRangeAsync(IEnumerable<GoogleTaskList> entities);

        Task<ResponseDto<GoogleTaskList>> FirstOrDefaultAsNoTrackingAsync(Expression<Func<GoogleTaskList, bool>> predicate);

        Task<ResponseDto<IEnumerable<GoogleTaskList>>> GetAsNoTrackingAsync(Expression<Func<GoogleTaskList, bool>> filter = null, Func<IQueryable<GoogleTaskList>, IOrderedQueryable<GoogleTaskList>> orderBy = null, string includeProperties = "");

        Task<EmptyResponseDto> RemoveAsync(GoogleTaskList entity);

        Task<EmptyResponseDto> RemoveRangeAsync(IEnumerable<GoogleTaskList> entities);

        Task<ResponseDto<GoogleTaskList>> UpdateAsync(GoogleTaskList entity);
    }
}
