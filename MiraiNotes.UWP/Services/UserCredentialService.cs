using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var credentialList = new List<PasswordCredential>();
            var credentials = vault.RetrieveAll();
            foreach (PasswordCredential credential in credentials)
            {
                credentialList.Add(vault.Retrieve(credential.Resource, credential.UserName));
            }
            foreach (PasswordCredential entry in credentialList)
            {
                vault.Remove(entry);
            }
        }

        private PasswordCredential GetUserCredential(string resource)
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
            var accessToken = GetUserCredential(TOKEN_RESOURCE);
            var refreshToken = GetUserCredential(REFRESH_TOKEN_RESOURCE);
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
                System.Diagnostics.Debug.WriteLine("User is not logged in");
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

        public void SaveUserCredentials(PasswordVaultResourceType resource, string username, string password)
        {
            SaveUserCredentials($"{resource}", username, password);
        }

        public string GetUserCredentials(PasswordVaultResourceType resource, string username)
        {
            PasswordCredential credential = null;
            try
            {
                var vault = new PasswordVault();
                // Try to get an existing credential from the vault.
                var credentials = vault.FindAllByResource($"{resource}");
                foreach (var item in credentials)
                {
                    if (item.UserName == username)
                    {
                        credential = item;
                        credential.RetrievePassword();
                    }
                }          
            }
            catch (Exception)
            {
            }
            return credential?.Password;
        }

        public void DeleteUserCredentials(PasswordVaultResourceType resource, string username)
        {
            var vault = new PasswordVault();
            var credentialList = new List<PasswordCredential>();

            try
            {
                var credentials = vault.FindAllByResource($"{resource}");
                foreach (PasswordCredential credential in credentials)
                {
                    if (credential.UserName == username)
                        credentialList.Add(vault.Retrieve(credential.Resource, credential.UserName));
                }
                foreach (PasswordCredential entry in credentialList)
                {
                    vault.Remove(entry);
                }
            }
            catch (Exception)
            {
                //Credential not found
            }
        }
    }
}
