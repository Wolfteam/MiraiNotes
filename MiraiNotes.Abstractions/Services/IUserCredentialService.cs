using MiraiNotes.Core.Enums;

namespace MiraiNotes.Abstractions.Services
{
    public interface IUserCredentialService
    {
        /// <summary>
        /// Its the default owner value for a resource.
        /// With this one, you can get the current logged username
        /// </summary>
        string DefaultUsername { get; }

        /// <summary>
        /// Gets the current logged username
        /// </summary>
        /// <returns>The logged username</returns>
        string GetCurrentLoggedUsername();

        /// <summary>
        /// Gets the secret associated to a resource and a username
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <param name="username">The current owner of the resource</param>
        /// <returns>The secret</returns>
        string GetUserCredential(ResourceType resource, string username);

        /// <summary>
        /// Removes the secret associated to a resource and a username
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <param name="username">The current owner of the resource</param>
        void DeleteUserCredential(ResourceType resource, string username);

        /// <summary>
        /// Saves a secret in the specified <paramref name="resource"/> and for the specified <paramref name="username"/>
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <param name="username">The current owner of the resource</param>
        /// <param name="secret">The secret</param>
        void SaveUserCredential(ResourceType resource, string username, string secret);

        /// <summary>
        /// Updates the owner of a resource or the secret associated to the <paramref name="resource"/>
        /// and the <paramref name="username"/>
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <param name="username">The current owner of the resource</param>
        /// <param name="updateUsername">If true <paramref name="newValue"/> will be used to update the owner of the resource, 
        /// otherwise it will update the secret
        /// </param>
        /// <param name="newValue">The new owner or the new secret to update</param>
        void UpdateUserCredential(ResourceType resource, string username, bool updateUsername, string newValue);
    }
}
