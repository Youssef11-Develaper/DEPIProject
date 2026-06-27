using Microsoft.EntityFrameworkCore;
using RegisistryV2.Data;
using RegisistryV2.Models;

namespace RegisistryV2.Services
{
    public class PeakTimeService
    {
        private readonly AppDbContext _context;

        public PeakTimeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(TimeSpan PeakStart, TimeSpan PeakEnd)> GetPeakTimeAsync(int branchId)
        {
            // جيب كل الحجوزات في الفرع ده
            var appointments = await _context.Appointments
                .Where(a => a.BranchId == branchId
                    && a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();

            // لو مفيش بيانات كافية ارجع null
            if (appointments.Count < 20)
                return (TimeSpan.Zero, TimeSpan.Zero);

            // قسم اليوم لـ slots كل ساعة وشوف أكتر slot فيه حجوزات
            var slotCounts = appointments
                .GroupBy(a => a.TimeSlot.Hours)
                .Select(g => new
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            if (!slotCounts.Any())
                return (TimeSpan.Zero, TimeSpan.Zero);

            // خد أعلى 30% من الأوقات كذروة
            var topCount = slotCounts.First().Count;
            var threshold = topCount * 0.7;

            var peakHours = slotCounts
                .Where(x => x.Count >= threshold)
                .Select(x => x.Hour)
                .OrderBy(h => h)
                .ToList();

            if (!peakHours.Any())
                return (TimeSpan.Zero, TimeSpan.Zero);

            var peakStart = new TimeSpan(peakHours.First(), 0, 0);
            var peakEnd = new TimeSpan(peakHours.Last() + 1, 0, 0);

            return (peakStart, peakEnd);
        }

        public async Task<DayOfWeek?> GetBusiestDayAsync(int branchId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.BranchId == branchId
                    && a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();

            if (appointments.Count < 20)
                return null;

            var busiestDay = appointments
                .GroupBy(a => a.AppointmentDate.DayOfWeek)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            return busiestDay?.Key;
        }

        public async Task<string> GetBusiestWeekPeriodAsync(int branchId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.BranchId == branchId
                    && a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();

            if (appointments.Count < 20)
                return null;

            // قسم الشهر لـ 3 فترات
            var firstWeek = appointments.Count(a => a.AppointmentDate.Day <= 10);
            var midMonth = appointments.Count(a => a.AppointmentDate.Day > 10
                && a.AppointmentDate.Day <= 20);
            var lastWeek = appointments.Count(a => a.AppointmentDate.Day > 20);

            if (firstWeek >= midMonth && firstWeek >= lastWeek)
                return "أول الشهر";
            else if (midMonth >= firstWeek && midMonth >= lastWeek)
                return "منتصف الشهر";
            else
                return "آخر الشهر";
        }
    }
}
