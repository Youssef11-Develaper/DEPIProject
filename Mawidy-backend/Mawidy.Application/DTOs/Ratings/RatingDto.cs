

namespace Mawidy.Application.DTOs.Ratings
{
    public class RatingDto
    {
        public int Id { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public int Stars { get; set; }
        public string? Comment { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

