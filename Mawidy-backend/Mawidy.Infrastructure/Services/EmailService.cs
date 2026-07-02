using Mawidy.Application.Interfaces;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;

namespace Mawidy.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendAsync(string toEmail, string toName, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort,
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendEmailConfirmationAsync(ApplicationUser user, string token)
        {
            var subject = "????? ?????? - ????? ??????";
            var body = $@"
        <div style='font-family:Arial,sans-serif;direction:rtl;padding:20px;max-width:600px;margin:0 auto'>
            <div style='background:linear-gradient(135deg,#1a5276,#2980b9);padding:20px;border-radius:12px 12px 0 0;text-align:center'>
                <h2 style='color:white;margin:0'>????? ?????? ??????</h2>
            </div>
            <div style='background:#f8f9fa;padding:30px;border-radius:0 0 12px 12px'>
                <h3 style='color:#1a5276'>????? {user.FirstName}!</h3>
                <p>????? ???????? ?????? ????? ?? ?????? ??????:</p>
                <div style='background:white;padding:20px;border-radius:12px;text-align:center;margin:20px 0'>
                    <h2 style='color:#1a5276;letter-spacing:5px'>{token}</h2>
                </div>
                <div style='background:#fff3cd;padding:15px;border-radius:8px'>
                    <p style='margin:0'>????? ?? ???? ???? 24 ???? ??</p>
                </div>
            </div>
        </div>";

            await SendAsync(user.Email!, user.FullName, subject, body);
        }

        public async Task SendAppointmentConfirmationAsync(ApplicationUser user, Appointment appointment)
        {
            var subject = "????? ??? ????? - ????? ??????";
            var body = $@"
        <div style='font-family:Arial,sans-serif;direction:rtl;padding:20px;max-width:600px;margin:0 auto'>
            <div style='background:linear-gradient(135deg,#1a5276,#2980b9);padding:20px;border-radius:12px 12px 0 0;text-align:center'>
                <h2 style='color:white;margin:0'>????? ?????? ??????</h2>
            </div>
            <div style='background:#f8f9fa;padding:30px;border-radius:0 0 12px 12px'>
                <h3 style='color:#1a5276'>????? {user.FirstName}!</h3>
                <p>?? ????? ??? ????? ?????</p>
                <div style='background:white;padding:20px;border-radius:12px;border-right:4px solid #2980b9;margin:20px 0'>
                    <p><strong>??????:</strong> {appointment.ServiceType.Name}</p>
                    <p><strong>?????:</strong> {appointment.Branch.Name}</p>
                    <p><strong>???????:</strong> {appointment.Branch.Address}</p>
                    <p><strong>???????:</strong> {appointment.AppointmentDate:dd/MM/yyyy}</p>
                    <p><strong>?????:</strong> {appointment.TimeSlot:hh\\:mm}</p>
                    <p><strong>??? ??????:</strong> #{appointment.Id}</p>
                </div>
                <div style='background:#d4edda;padding:15px;border-radius:8px'>
                    <p style='margin:0'>???? ?????? ??? ????? ?? 10 ?????</p>
                </div>
            </div>
        </div>";

            await SendAsync(user.Email!, user.FullName, subject, body);
        }

        public async Task SendAppointmentReminderAsync(ApplicationUser user, Appointment appointment)
        {
            var subject = "????? ?????? ???? - ????? ??????";
            var body = $@"
        <div style='font-family:Arial,sans-serif;direction:rtl;padding:20px;max-width:600px;margin:0 auto'>
            <div style='background:linear-gradient(135deg,#1a5276,#2980b9);padding:20px;border-radius:12px 12px 0 0;text-align:center'>
                <h2 style='color:white;margin:0'>????? ?????? ??????</h2>
            </div>
            <div style='background:#f8f9fa;padding:30px;border-radius:0 0 12px 12px'>
                <h3 style='color:#1a5276'>????? ?????? ????</h3>
                <div style='background:white;padding:20px;border-radius:12px;border-right:4px solid #27ae60;margin:20px 0'>
                    <p><strong>??????:</strong> {appointment.ServiceType.Name}</p>
                    <p><strong>?????:</strong> {appointment.Branch.Name}</p>
                    <p><strong>?????:</strong> {appointment.TimeSlot:hh\\:mm}</p>
                    <p><strong>??? ??????:</strong> #{appointment.Id}</p>
                </div>
                <div style='background:#d4edda;padding:15px;border-radius:8px'>
                    <p style='margin:0'>???? ????? ????? ????? ?????? ?????????? ????????</p>
                </div>
            </div>
        </div>";

            await SendAsync(user.Email!, user.FullName, subject, body);
        }

        public async Task SendPasswordResetAsync(ApplicationUser user, string token)
        {
            var subject = "????? ????? ???? ?????? - ????? ??????";
            var body = $@"
        <div style='font-family:Arial,sans-serif;direction:rtl;padding:20px;max-width:600px;margin:0 auto'>
            <div style='background:linear-gradient(135deg,#1a5276,#2980b9);padding:20px;border-radius:12px 12px 0 0;text-align:center'>
                <h2 style='color:white;margin:0'>????? ?????? ??????</h2>
            </div>
            <div style='background:#f8f9fa;padding:30px;border-radius:0 0 12px 12px'>
                <h3 style='color:#1a5276'>????? ????? ???? ??????</h3>
                <p>?????? ????? ?? ?????? ????? ???? ??????:</p>
                <div style='background:white;padding:20px;border-radius:12px;text-align:center;margin:20px 0'>
                    <h2 style='color:#1a5276;letter-spacing:5px'>{token}</h2>
                </div>
                <div style='background:#fff3cd;padding:15px;border-radius:8px'>
                    <p style='margin:0'>????? ?? ???? ???? 24 ???? ??</p>
                </div>
                <p style='color:#999;font-size:12px;margin-top:20px'>
                    ?? ?? ??? ???? ???? ??? ????? ???????
                </p>
            </div>
        </div>";

            await SendAsync(user.Email!, user.FullName, subject, body);
        }

        public async Task SendComplaintResponseAsync(ApplicationUser user, Complaint complaint)
        {
            var subject = "?? ??? ????? - ????? ??????";
            var body = $@"
        <div style='font-family:Arial,sans-serif;direction:rtl;padding:20px;max-width:600px;margin:0 auto'>
            <div style='background:linear-gradient(135deg,#1a5276,#2980b9);padding:20px;border-radius:12px 12px 0 0;text-align:center'>
                <h2 style='color:white;margin:0'>????? ?????? ??????</h2>
            </div>
            <div style='background:#f8f9fa;padding:30px;border-radius:0 0 12px 12px'>
                <h3 style='color:#1a5276'>?? ???? ??? ?????</h3>
                <div style='background:white;padding:20px;border-radius:12px;margin:20px 0'>
                    <p><strong>????? ??????:</strong> {complaint.Title}</p>
                    <p><strong>??????:</strong> {GetStatusText(complaint.Status)}</p>
                </div>
                <div style='background:#e8f4fd;padding:15px;border-radius:8px'>
                    <p><strong>?? ???????:</strong></p>
                    <p>{complaint.AdminResponse}</p>
                </div>
            </div>
        </div>";

            await SendAsync(user.Email!, user.FullName, subject, body);
        }

        private string GetStatusText(ComplaintStatus status) => status switch
        {
            ComplaintStatus.Submitted => "?? ???????",
            ComplaintStatus.UnderReview => "??? ????????",
            ComplaintStatus.Resolved => "?? ????",
            ComplaintStatus.Closed => "?????",
            _ => ""
        };
    }
}

