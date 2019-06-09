using MiraiNotes.Data.Models;
using MiraiNotes.Shared.Models;
using System.Threading.Tasks;

namespace MiraiNotes.DataService.Interfaces
{
    public interface IUserDataService : IRepository<GoogleUser>
    {
        Task<Response<GoogleUser>> GetCurrentActiveUserAsync();

        Task<EmptyResponse> ChangeCurrentUserStatus(bool isActive);

        Task<EmptyResponse> SetAsCurrentUser(string email);
    }
}
