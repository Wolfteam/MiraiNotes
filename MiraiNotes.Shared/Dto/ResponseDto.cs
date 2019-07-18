namespace MiraiNotes.Shared.Models
{
    public class ResponseDto<T> : EmptyResponseDto
    {
        public T Result { get; set; }
    }
}
