using Mawidy.Application.Interfaces;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using QRCoder;

namespace Mawidy.Infrastructure.Services
{
    public class QRService : IQRService
    {
        public string GenerateQRCode(Appointment appointment, int queueNumber)
        {
            var data = $@"??? ?????: #{appointment.Id}
??? ???????: {queueNumber}
?????: {appointment.User.FullName}
????? ??????: {appointment.User.NationalId}
??????: {appointment.ServiceType.Name}
?????: {appointment.Branch.Name}
???????: {appointment.AppointmentDate:dd/MM/yyyy}
?????: {appointment.TimeSlot:hh\:mm}";

            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(10);
            return Convert.ToBase64String(qrCodeBytes);
        }
    }
}

