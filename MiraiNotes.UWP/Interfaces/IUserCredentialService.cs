using MiraiNotes.UWP.Models;
using Windows.Security.Credentials;

namespace MiraiNotes.UWP.Interfaces
{
    public interface IUserCredentialService
    {
        void SaveUserCredentials(PasswordVaultResourceType resource, string username, string password);

        void SaveUserCredentials(string resource, string username, string password);

        void SaveUserCredentials(string email, TokenResponse token);

        bool IsUserLoggedIn();

        string GetUserCredentials(PasswordVaultResourceType resource, string username);

        TokenResponse GetUserToken();

        void DeleteUserCredentials();

        void DeleteUserCredentials(PasswordVaultResourceType resource, string username);
    }
}
