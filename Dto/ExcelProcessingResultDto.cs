using System.Diagnostics.CodeAnalysis;

namespace Dto
{
    [ExcludeFromCodeCoverage]
    public class ExcelProcessingResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessfulUploads { get; set; }
        public int FailedUploads { get; set; }
        public List<ErrorResponseDto>? ErrorResponses { get; set; }
    }
}