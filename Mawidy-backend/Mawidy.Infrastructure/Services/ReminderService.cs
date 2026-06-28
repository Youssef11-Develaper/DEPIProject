using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Mawidy.Application.Interfaces;

namespace Mawidy.Infrastructure.Services
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
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private async Task SendReminders()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var tomorrow = DateTime.Today.AddDays(1);

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
