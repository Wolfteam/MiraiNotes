using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Enums;
using System;
using Xamarin.Essentials;

namespace MiraiNotes.Android.Services
{
    public class UserCredentialService : IUserCredentialService
    {
        public string DefaultUsername => "DEFAULT_USERNAME";

        public UserCredentialService()
        {
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
            return SecureStorage.GetAsync(key).GetAwaiter().GetResult();
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
                        SecureStorage.Remove(key);
                    }

                    key = $"{resourceName}_{username}";
                    SecureStorage.Remove(key);
                }

                return;
            }

            key = $"{resource}_{username}";
            SecureStorage.Remove(key);
        }

        public void SaveUserCredential(ResourceType resource, string username, string secret)
        {
            if (resource == ResourceType.ALL)
                throw new ArgumentOutOfRangeException(
                    nameof(resource), resource,
                    $"Cant save a resource of type {resource}");

            var key = $"{resource}_{username}";
            SecureStorage.SetAsync(key, secret).GetAwaiter().GetResult();
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