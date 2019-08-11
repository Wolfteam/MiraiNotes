namespace MiraiNotes.Core.Dto
{
    public class ResponseDto<T> : EmptyResponseDto
    {
        public T Result { get; set; }
    }
}
