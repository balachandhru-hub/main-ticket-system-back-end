using System;
using System.ComponentModel.DataAnnotations;

namespace Dto
{
    public class SchedulerHistoryResponseDto
    {
        public string SchedulerKey { get; set; }
        public DateTime? LastEndDate { get; set; }
        public DateTime? NextStartDate { get; set; }  // LastEndDate + 10 minutes
    }
}