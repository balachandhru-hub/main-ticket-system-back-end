using System;

namespace Dto
{
    /// <summary>
    /// Standard response DTO for API operations.
    /// </summary>
    public class ResponseDto
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
    }
}