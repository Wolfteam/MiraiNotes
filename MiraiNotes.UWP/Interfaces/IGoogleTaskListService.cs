using System.Threading.Tasks;
using MiraiNotes.Core.Models.GoogleApi;

namespace MiraiNotes.UWP.Interfaces
{
    public interface IGoogleTaskListService
    {
        Task<GoogleResponseModel<GoogleTaskApiResponseModel<GoogleTaskListModel>>> GetAllAsync(int maxResults = 100, string pageToken = null);

        Task<GoogleResponseModel<GoogleTaskListModel>> GetAsync(string taskListID);

        Task<GoogleResponseModel<GoogleTaskListModel>> SaveAsync(GoogleTaskListModel taskList);

        Task<GoogleResponseModel<GoogleTaskListModel>> UpdateAsync(string taskListID, GoogleTaskListModel taskList);

        Task<GoogleEmptyResponseModel> DeleteAsync(string taskListID);
    }
}
