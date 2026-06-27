using CivilRegistryAPI.Data;
using CivilRegistryAPI.DTOs.Appointments;
using CivilRegistryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CivilRegistryAPI.Services
{
    public class AppointmentAvailabilityService
    {
        private readonly AppDbContext _context;
        private readonly PeakTimeService _peakTimeService;

        public AppointmentAvailabilityService(
            AppDbContext context,
            PeakTimeService peakTimeService)
        {
            _context = context;
            _peakTimeService = peakTimeService;
        }

        public async Task<AvailableSlotsDto> BuildSlotViewModelAsync(
            int branchId, int serviceTypeId, DateTime selectedDate)
        {
            var branch = await _context.Branches
                .Include(b => b.Schedules)
                .FirstOrDefaultAsync(b => b.Id == branchId);

            var serviceType = await _context.ServiceTypes.FindAsync(serviceTypeId);

            if (branch == null || serviceType == null)
                throw new InvalidOperationException("الفرع أو الخدمة غير موجودة");

            var viewModel = new AvailableSlotsDto
            {
                BranchId = branchId,
                BranchName = branch.Name,
                ServiceTypeId = serviceTypeId,
                ServiceName = serviceType.Name,
                SelectedDate = selectedDate,
                IsAvailable = true
            };

            // هل الفرع إجازة؟
            var holiday = await _context.BranchHolidays
                .FirstOrDefaultAsync(h => h.BranchId == branchId
                    && h.Date.Date == selectedDate.Date);

            if (holiday != null)
            {
                viewModel.IsAvailable = false;
                viewModel.UnavailabilityMessage = $"الفرع إجازة النهارده - {holiday.Reason}";
                return viewModel;
            }

            // هل الخدمة معطلة؟
            var unavailability = await _context.ServiceUnavailabilities
                .Include(s => s.ServiceType)
                .FirstOrDefaultAsync(s => s.BranchId == branchId
                    && s.ServiceTypeId == serviceTypeId
                    && s.Date.Date == selectedDate.Date);

            if (unavailability != null)
            {
                viewModel.IsAvailable = false;
                viewModel.UnavailabilityMessage =
                    $"خدمة {serviceType.Name} مش متاحة النهارده - {unavailability.Reason}";
                return viewModel;
            }

            // هل الفرع بيشتغل في اليوم ده؟
            var schedule = branch.Schedules
                .FirstOrDefault(s => s.DayOfWeek == selectedDate.DayOfWeek);

            if (schedule == null)
            {
                viewModel.IsAvailable = false;
                viewModel.UnavailabilityMessage = "الفرع مش بيشتغل في اليوم ده";
                return viewModel;
            }

            // جيب الحجوزات
            var bookedAppointments = await _context.Appointments
                .Where(a => a.BranchId == branchId
                    && a.AppointmentDate.Date == selectedDate.Date
                    && a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();

            // احسب الذروة
            var (peakStart, peakEnd) = await _peakTimeService.GetPeakTimeAsync(branchId);
            if (peakStart == TimeSpan.Zero && peakEnd == TimeSpan.Zero)
            {
                peakStart = schedule.PeakStartTime;
                peakEnd = schedule.PeakEndTime;
            }

            // اعمل الـ Slots
            var slots = new List<SlotDto>();
            var current = schedule.OpenTime;

            while (current < schedule.CloseTime)
            {
                var bookedCount = bookedAppointments.Count(a => a.TimeSlot == current);

                slots.Add(new SlotDto
                {
                    Time = current,
                    BookedCount = bookedCount,
                    MaxCount = schedule.MaxAppointmentsPerSlot,
                    IsBooked = bookedCount >= schedule.MaxAppointmentsPerSlot,
                    IsPeak = current >= peakStart && current < peakEnd
                });

                current = current.Add(TimeSpan.FromMinutes(serviceType.DurationMinutes));
            }

            viewModel.Slots = slots;

            // إحصائيات الازدحام
            var busiestDay = await _peakTimeService.GetBusiestDayAsync(branchId);
            viewModel.BusiestDayName = busiestDay.HasValue
                ? _peakTimeService.GetDayName(busiestDay.Value)
                : null;
            viewModel.BusiestPeriod = await _peakTimeService.GetBusiestWeekPeriodAsync(branchId);

            return viewModel;
        }
    }
}
