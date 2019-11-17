using System;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;

namespace MiraiNotes.Android.Services
{
    public class UserCredentialService : IUserCredentialService
    {
        private readonly IAndroidAppSettings _appSettings;
        
        public string DefaultUsername => "DEFAULT_USERNAME";

        public UserCredentialService(IAndroidAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public string GetCurrentLoggedUsername()
        {
            return GetUserCredential(ResourceType.LOGGED_USER_RESOURCE, DefaultUsername);
        }

        public string GetUserCredential(ResourceType resource, string username)
        {
            if (resource == ResourceType.ALL)
                throw new ArgumentOutOfRangeException(
                    nameof(resource), resource,
                    $"Cant retrieve a resource of type {resource}");

            var key = $"{resource}_{username}";
            return _appSettings.GetString(key);
        }

        public void DeleteUserCredential(ResourceType resource, string username)
        {
            string key;
            if (resource == ResourceType.ALL)
            {
                var resourceNames = Enum.GetNames(typeof(ResourceType));
                foreach (var resourceName in resourceNames)
                {
                    if (DefaultUsername != username)
                    {
                        key = $"{resourceName}_{DefaultUsername}";
                        _appSettings.SetString(key, null);
                    }

                    key = $"{resourceName}_{username}";
                    _appSettings.SetString(key, null);
                }

                return;
            }

            key = $"{resource}_{username}";
            _appSettings.SetString(key, null);
        }

        public void SaveUserCredential(ResourceType resource, string username, string secret)
        {
            if (resource == ResourceType.ALL)
                throw new ArgumentOutOfRangeException(
                    nameof(resource), resource,
                    $"Cant save a resource of type {resource}");

            var key = $"{resource}_{username}";
            _appSettings.SetString(key, secret);
        }

        public void UpdateUserCredential(ResourceType resource, string username, bool updateUsername, string newValue)
        {
            if (resource == ResourceType.ALL)
                throw new ArgumentOutOfRangeException(
                    nameof(resource), resource,
                    $"Cant update a resource of type {resource}");

            //If i need to update the username of the secret
            if (updateUsername)
            {
                var currentSecret = GetUserCredential(resource, username);
                DeleteUserCredential(resource, username);
                SaveUserCredential(resource, newValue, currentSecret);
            }
            else
            {
                DeleteUserCredential(resource, username);
                SaveUserCredential(resource, username, newValue);
            }
        }
    }
}