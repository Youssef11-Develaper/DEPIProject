using Mawidy.Application.Interfaces;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Mawidy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mawidy.Infrastructure.Services
{
    public class PeakTimeService : IPeakTimeService
    {
        private readonly AppDbContext _context;

        private static readonly Dictionary<DayOfWeek, string> DayNames = new()
    {
        { DayOfWeek.Sunday, "?????" },
        { DayOfWeek.Monday, "???????" },
        { DayOfWeek.Tuesday, "????????" },
        { DayOfWeek.Wednesday, "????????" },
        { DayOfWeek.Thursday, "??????" },
        { DayOfWeek.Friday, "??????" },
        { DayOfWeek.Saturday, "?????" }
    };

        public PeakTimeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(TimeSpan PeakStart, TimeSpan PeakEnd)> GetPeakTimeAsync(int branchId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.BranchId == branchId
                    && a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();

            if (appointments.Count < 20)
                return (TimeSpan.Zero, TimeSpan.Zero);

            var slotCounts = appointments
                .GroupBy(a => a.TimeSlot.Hours)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            if (!slotCounts.Any())
                return (TimeSpan.Zero, TimeSpan.Zero);

            var threshold = slotCounts.First().Count * 0.7;

            var peakHours = slotCounts
                .Where(x => x.Count >= threshold)
                .Select(x => x.Hour)
                .OrderBy(h => h)
                .ToList();

            if (!peakHours.Any())
                return (TimeSpan.Zero, TimeSpan.Zero);

            return (
                new TimeSpan(peakHours.First(), 0, 0),
                new TimeSpan(peakHours.Last() + 1, 0, 0)
            );
        }

        public async Task<DayOfWeek?> GetBusiestDayAsync(int branchId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.BranchId == branchId
                    && a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();

            if (appointments.Count < 20) return null;

            return appointments
                .GroupBy(a => a.AppointmentDate.DayOfWeek)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key;
        }

        public async Task<string?> GetBusiestWeekPeriodAsync(int branchId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.BranchId == branchId
                    && a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();

            if (appointments.Count < 20) return null;

            var firstWeek = appointments.Count(a => a.AppointmentDate.Day <= 10);
            var midMonth = appointments.Count(a => a.AppointmentDate.Day > 10
                && a.AppointmentDate.Day <= 20);
            var lastWeek = appointments.Count(a => a.AppointmentDate.Day > 20);

            if (firstWeek >= midMonth && firstWeek >= lastWeek) return "??? ?????";
            if (midMonth >= firstWeek && midMonth >= lastWeek) return "????? ?????";
            return "??? ?????";
        }

        public string GetDayName(DayOfWeek day)
            => DayNames.TryGetValue(day, out var name) ? name : "";
    }
}

