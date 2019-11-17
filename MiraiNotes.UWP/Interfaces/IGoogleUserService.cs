using System.Threading.Tasks;
using MiraiNotes.Core.Dto.Google.Responses;

namespace MiraiNotes.UWP.Interfaces
{
    public interface IGoogleUserService
    {
        /// <summary>
        /// Gets the google user info of the current logged user, if it fails
        /// it returns null
        /// </summary>
        /// <returns><see cref="Task{GoogleUserModel}"/></returns>
        Task<GoogleUserResponseDto> GetUserInfoAsync();

        /// <summary>
        /// Removes the local profile image of the user specified in <paramref name="googleUserId"/>
        /// </summary>
        /// <returns>Task</returns>
        Task RemoveProfileImage(string googleUserId);

        /// <summary>
        /// Downloads the profile image specified by <paramref name="url"/>
        /// </summary>
        /// <param name="url">Url of the image</param>
        /// <returns>Task</returns>
        Task DownloadProfileImage(string url, string googleUserId);

        /// <summary>
        /// Gets the full path of the profile image for user specified
        /// in <paramref name="googleUserId"/>
        /// </summary>
        /// <returns>The full image path</returns>
        string GetUserProfileImagePath(string googleUserId);
    }
}
