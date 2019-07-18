using MiraiNotes.Data.Models;
using MiraiNotes.Shared.Models;
using System.Threading.Tasks;

namespace MiraiNotes.DataService.Interfaces
{
    public interface IUserDataService : IRepository<GoogleUser>
    {
        Task<ResponseDto<GoogleUser>> GetCurrentActiveUserAsync();

        Task<EmptyResponseDto> ChangeCurrentUserStatus(bool isActive);

        Task<EmptyResponseDto> SetAsCurrentUser(string email);
    }
}
