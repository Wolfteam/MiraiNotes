using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;

namespace MiraiNotes.Abstractions.Data
{
    public interface IUserDataService
    {
        Task<ResponseDto<GoogleUser>> AddAsync(GoogleUser entity);

        Task<ResponseDto<bool>> ExistsAsync(Expression<Func<GoogleUser, bool>> predicate);

        Task<ResponseDto<GoogleUser>> FirstOrDefaultAsNoTrackingAsync(Expression<Func<GoogleUser, bool>> predicate);

        Task<ResponseDto<IEnumerable<GoogleUser>>> GetAllAsNoTrackingAsync();

        Task<ResponseDto<IEnumerable<GoogleUser>>> GetAsNoTrackingAsync(Expression<Func<GoogleUser, bool>> filter = null, Func<IQueryable<GoogleUser>, IOrderedQueryable<GoogleUser>> orderBy = null, string includeProperties = "");

        Task<ResponseDto<IEnumerable<GoogleUser>>> GetAsync(Expression<Func<GoogleUser, bool>> filter = null, Func<IQueryable<GoogleUser>, IOrderedQueryable<GoogleUser>> orderBy = null, string includeProperties = "");

        Task<EmptyResponseDto> RemoveAsync(object id);

        Task<ResponseDto<GoogleUser>> UpdateAsync(GoogleUser entity);

        Task<ResponseDto<GoogleUser>> GetCurrentActiveUserAsync();

        Task<EmptyResponseDto> ChangeCurrentUserStatus(bool isActive);

        Task<EmptyResponseDto> SetAsCurrentUser(string email);
    }
}
