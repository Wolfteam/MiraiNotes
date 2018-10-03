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
        #region Properties
        public string DefaultUsername => "DEFAULT_USERNAME";
        #endregion

        #region Methods
        public string GetCurrentLoggedUsername()
            => GetUserCredential(PasswordVaultResourceType.LOGGED_USER_RESOURCE, DefaultUsername);

        public void SaveUserCredential(PasswordVaultResourceType resource, string username, string password)
        {
            if (resource == PasswordVaultResourceType.ALL)
                throw new ArgumentOutOfRangeException(nameof(resource), resource, $"Cant save a resource of type {resource}");
            SaveUserCredentials($"{resource}", username, password);
        }

        public string GetUserCredential(PasswordVaultResourceType resource, string username)
        {
            if (resource == PasswordVaultResourceType.ALL)
                throw new ArgumentOutOfRangeException(nameof(resource), resource, $"Cant retrieve a resource of type {resource}");

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

        public void DeleteUserCredential(PasswordVaultResourceType resource, string username)
        {
            if (resource == PasswordVaultResourceType.ALL)
            {
                if (DefaultUsername != username)
                    DeleteUserCredentials(DefaultUsername);
                DeleteUserCredentials(username);
                return;
            }

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

        public void UpdateUserCredential(PasswordVaultResourceType resource, string username, bool updateUsername, string newValue)
        {
            if (resource == PasswordVaultResourceType.ALL)
                throw new ArgumentOutOfRangeException(nameof(resource), resource, $"Cant update a resource of type {resource}");

            //If i need to update the username of the secret
            if (updateUsername)
            {
                string currentSecret = GetUserCredential(resource, username);
                DeleteUserCredential(resource, username);
                SaveUserCredential(resource, newValue, currentSecret);
            }
            else
            {
                DeleteUserCredential(resource, username);
                SaveUserCredential(resource, username, newValue);
            }
        }

        private void DeleteUserCredentials(string username)
        {
            var vault = new PasswordVault();
            var credentialList = new List<PasswordCredential>();
            var credentials = vault.RetrieveAll();
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

        private void SaveUserCredentials(string resource, string username, string password)
        {
            PasswordVault vault = new PasswordVault();
            PasswordCredential credential = new PasswordCredential(resource, username, password);
            vault.Add(credential);
        }
        #endregion
    }
}
