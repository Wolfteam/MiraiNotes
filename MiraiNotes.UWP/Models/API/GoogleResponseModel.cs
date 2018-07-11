namespace MiraiNotes.UWP.Models.API
{
    public class GoogleResponseModel<T>
    {
        public bool Succeed { get; set; }
        public T Result { get; set; }
        public GoogleResponseErrorModel Errors { get; set; }
    }
}
