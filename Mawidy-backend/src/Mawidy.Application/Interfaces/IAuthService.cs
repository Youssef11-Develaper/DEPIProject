using System;
using System.Collections.Generic;
using System.Text;
using Mawidy.Application.DTOs.Auth;

namespace Mawidy.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string userId);
    }
}
