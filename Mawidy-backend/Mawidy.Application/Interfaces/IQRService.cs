using Mawidy.Domain.Entities;

namespace Mawidy.Application.Interfaces
{
    public interface IQRService
    {
        string GenerateQRCode(Appointment appointment, int queueNumber);
    }
}
