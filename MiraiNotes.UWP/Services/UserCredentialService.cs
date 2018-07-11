using System;
using System.Linq;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using Windows.Security.Credentials;

namespace MiraiNotes.UWP.Services
{
    public class UserCredentialService : IUserCredentialService
    {
        private const string LOGGED_USER_RESOURCE = "LOGGED_USER_RESOURCE";
        private const string TOKEN_RESOURCE = "TOKEN_RESOURCE";
        private const string REFRESH_TOKEN_RESOURCE = "REFRESH_TOKEN_RESOURCE";

        public void DeleteUserCredentials()
        {
            var vault = new PasswordVault();
            var credentials = vault.RetrieveAll();
            foreach (var credential in credentials)
            {
                vault.Remove(credential);
            }
        }

        public PasswordCredential GetUserCredentials(string resource)
        {
            PasswordCredential credential = null;
            try
            {
                var vault = new PasswordVault();
                // Try to get an existing credential from the vault.
                credential = vault.FindAllByResource(resource).FirstOrDefault();
                credential.RetrievePassword();
            }
            catch (Exception)
            {
                // When there is no matching resource an error occurs, which we ignore.
            }
            return credential;
        }

        public TokenResponse GetUserToken()
        {
            var accessToken = GetUserCredentials(TOKEN_RESOURCE);
            var refreshToken = GetUserCredentials(REFRESH_TOKEN_RESOURCE);
            return new TokenResponse
            {
                AccessToken = accessToken?.Password,
                RefreshToken = refreshToken?.Password,
            };
        }

        public bool IsUserLoggedIn()
        {
            bool isUserLogged = false;
            try
            {
                var vault = new PasswordVault();
                // Try to get an existing credential from the vault.
                var credential = vault.FindAllByResource(LOGGED_USER_RESOURCE).FirstOrDefault();
                isUserLogged = credential != null;
            }
            catch (Exception)
            {
                // When there is no matching resource an error occurs, which we ignore.
            }
            return isUserLogged;
        }

        public void SaveUserCredentials(string resource, string username, string password)
        {
            PasswordVault vault = new PasswordVault();
            PasswordCredential credential = new PasswordCredential(resource, username, password);
            vault.Add(credential);
        }

        public void SaveUserCredentials(string username, TokenResponse token)
        {
            DeleteUserCredentials();
            if (string.IsNullOrEmpty(username))
                username = LOGGED_USER_RESOURCE;
            SaveUserCredentials(LOGGED_USER_RESOURCE, username, "default");
            SaveUserCredentials(REFRESH_TOKEN_RESOURCE, username, token.RefreshToken);
            SaveUserCredentials(TOKEN_RESOURCE, username, token.AccessToken);
        }
    }
}
