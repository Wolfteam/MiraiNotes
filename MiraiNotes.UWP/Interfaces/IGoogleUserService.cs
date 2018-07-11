using MiraiNotes.UWP.Models;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Interfaces
{
    public interface IGoogleUserService
    {
        Task<GoogleUserModel> GetUserInfoAsync(string token);
    }
}
