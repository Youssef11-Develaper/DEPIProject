using System;
using System.Collections.Generic;
using System.Text;

namespace Mawidy.Application.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
    }
}
