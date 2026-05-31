using System;
using System.Collections.Generic;
using System.Text;

namespace Mawidy.Application.DTOs.Auth
{
    public class UserInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
