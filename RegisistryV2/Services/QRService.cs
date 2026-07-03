using QRCoder;
using RegisistryV2.Models;

namespace RegisistryV2.Services
{
    public class QRService
    {
        public string GenerateQRCode(Appointment appointment, int queueNumber)
        {
            var data = $@"
        رقم الحجز: #{appointment.Id}
        رقم الطابور: {queueNumber}
        الاسم: {appointment.User.FullName}
        الخدمة: {appointment.ServiceType.Name}
        الفرع: {appointment.Branch.Name}
        التاريخ: {appointment.AppointmentDate:dd/MM/yyyy}
        الوقت: {appointment.TimeSlot:hh\:mm}
    ";

            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(10);
            return Convert.ToBase64String(qrCodeBytes);
        }
    }   
}
