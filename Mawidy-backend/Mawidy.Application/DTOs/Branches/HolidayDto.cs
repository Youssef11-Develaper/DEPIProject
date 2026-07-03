using System;

namespace Mawidy.Application.DTOs.Branches
{
    public class HolidayDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
