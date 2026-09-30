using System;
using System.ComponentModel.DataAnnotations;
using Entities.Common;

namespace Dto
{
    public class TicketPRCreateDto
    {

        public string TaskName { get; set; }


        public double Hrs { get; set; }


        public decimal Amount { get; set; }
    }
}