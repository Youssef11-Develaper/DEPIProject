using Mawidy.Domain.Entities;

namespace Mawidy.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string toName, string subject, string body);
        Task SendEmailConfirmationAsync(ApplicationUser user, string token);
        Task SendAppointmentConfirmationAsync(ApplicationUser user, Appointment appointment);
        Task SendAppointmentReminderAsync(ApplicationUser user, Appointment appointment);
        Task SendPasswordResetAsync(ApplicationUser user, string token);
        Task SendComplaintResponseAsync(ApplicationUser user, Complaint complaint);
    }
}
