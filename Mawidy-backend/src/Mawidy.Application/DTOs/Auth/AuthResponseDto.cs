using System;
using System.Collections.Generic;
using System.Text;

namespace Mawidy.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public UserInfoDto? User { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
