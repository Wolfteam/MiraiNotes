namespace MiraiNotes.Core.Models.GoogleApi
{
    public class GoogleEmptyResponseModel
    {
        public bool Succeed { get; set; }
        public GoogleResponseErrorModel Errors { get; set; }
    }
}
