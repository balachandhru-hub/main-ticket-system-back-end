namespace Dto
{
    public class BatchProcessingResultDto
    {
        public int SuccessfulCount { get; set; }
        public int FailedCount { get; set; }
        public List<ErrorResponseDto> Errors { get; set; } = new List<ErrorResponseDto>();
    }
}