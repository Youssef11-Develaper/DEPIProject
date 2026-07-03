using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RegisistryV2.Data;
using RegisistryV2.Models;

namespace RegisistryV2.Services
{
    public class ReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ReminderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await SendReminders();

                // بيشتغل كل 12 ساعة
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private async Task SendReminders()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var tomorrow = DateTime.Today.AddDays(1);

            // جيب المواعيد اللي بكره ولسه مبعتلهاش تذكير
            var appointments = await context.Appointments
                .Include(a => a.Branch)
                .Include(a => a.ServiceType)
                .Where(a => a.AppointmentDate.Date == tomorrow.Date
                    && a.Status == AppointmentStatus.Confirmed
                    && !a.IsNotified)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                var user = await userManager.FindByIdAsync(appointment.UserId);
                if (user == null) continue;

                try
                {
                    await emailService.SendAppointmentReminderAsync(user, appointment);
                    appointment.IsNotified = true;
                }
                catch { }
            }

            await context.SaveChangesAsync();
        }
    }
}
