using MiraiNotes.UWP.Models;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Interfaces
{
    public interface IGoogleUserService
    {
        /// <summary>
        /// Gets the google user info of the current logged user, if it fails
        /// it returns null
        /// </summary>
        /// <returns><see cref="Task{GoogleUserModel}"/></returns>
        Task<GoogleUserModel> GetUserInfoAsync();

        /// <summary>
        /// Removes the profile image of the user
        /// </summary>
        /// <returns>Task</returns>
        Task RemoveProfileImage();

        /// <summary>
        /// Downloads the profile image specified by <paramref name="url"/>
        /// </summary>
        /// <param name="url">Url of the image</param>
        /// <returns>Task</returns>
        Task DownloadProfileImage(string url);

        /// <summary>
        /// Gets the full path of the profile image for the current logged user
        /// </summary>
        /// <returns>The full image path</returns>
        string GetCurrentUserProfileImagePath();
    }
}
