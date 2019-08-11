using System.Threading.Tasks;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;

namespace MiraiNotes.Abstractions.Data
{
    public interface IUserDataService : IRepository<GoogleUser>
    {
        Task<ResponseDto<GoogleUser>> GetCurrentActiveUserAsync();

        Task<EmptyResponseDto> ChangeCurrentUserStatus(bool isActive);

        Task<EmptyResponseDto> SetAsCurrentUser(string email);
    }
}
