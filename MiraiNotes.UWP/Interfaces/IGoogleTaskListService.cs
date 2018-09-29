using MiraiNotes.UWP.Models.API;
using System.Threading.Tasks;

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
