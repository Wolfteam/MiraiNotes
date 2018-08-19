using MiraiNotes.Data.Models;
using System.Threading.Tasks;

namespace MiraiNotes.DataService.Interfaces
{
    public interface IUserDataService : IRepository<GoogleUser>
    {
        Task<GoogleUser> GetCurrentActiveUserAsync();

        Task ChangeCurrentUserStatus(bool isActive);
    }
}
