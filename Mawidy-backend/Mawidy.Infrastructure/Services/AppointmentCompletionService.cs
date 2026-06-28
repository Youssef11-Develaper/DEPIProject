using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;

namespace Mawidy.Infrastructure.Services
{
    public class AppointmentCompletionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AppointmentCompletionService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CompleteExpiredAppointments();
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task CompleteExpiredAppointments()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.Now;
            var tenMinutesAgo = now.AddMinutes(-10);

            var appointments = await context.Appointments
                .Where(a => a.Status == AppointmentStatus.Confirmed
                    && a.AppointmentDate.Date == now.Date)
                .ToListAsync();

                       appointments = appointments
                           .Where(a => a.AppointmentDate.Date.Add(a.TimeSlot) <= tenMinutesAgo)
                           .ToList();

            foreach (var appointment in appointments)
                appointment.Status = AppointmentStatus.Completed;

            if (appointments.Any())
                await context.SaveChangesAsync();
        }
    }
}
