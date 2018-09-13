namespace MiraiNotes.Shared.Models
{
    public class Response<T> : EmptyResponse
    {
        public T Result { get; set; }
    }
}
