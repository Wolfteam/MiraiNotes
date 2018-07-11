using MiraiNotes.UWP.Models;
using Windows.Security.Credentials;

namespace MiraiNotes.UWP.Interfaces
{
    public interface IUserCredentialService
    {
        void SaveUserCredentials(string resource, string username, string password);
        void SaveUserCredentials(string email, TokenResponse token);
        bool IsUserLoggedIn();
        PasswordCredential GetUserCredentials(string resource);
        TokenResponse GetUserToken();
        void DeleteUserCredentials();
    }
}
