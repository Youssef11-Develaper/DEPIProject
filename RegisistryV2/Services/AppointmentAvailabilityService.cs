using Microsoft.EntityFrameworkCore;
using RegisistryV2.Data;
using RegisistryV2.Models;
using RegisistryV2.ViewModel.Appointment;

namespace RegisistryV2.Services
{
    public class AppointmentAvailabilityService
    {
        private readonly AppDbContext _context;
        private readonly PeakTimeService _peakTimeService;

        private static readonly Dictionary<DayOfWeek, string> DayNames = new()
    {
        { DayOfWeek.Sunday, "الأحد" },
        { DayOfWeek.Monday, "الاثنين" },
        { DayOfWeek.Tuesday, "الثلاثاء" },
        { DayOfWeek.Wednesday, "الأربعاء" },
        { DayOfWeek.Thursday, "الخميس" },
        { DayOfWeek.Friday, "الجمعة" },
        { DayOfWeek.Saturday, "السبت" }
    };

        public AppointmentAvailabilityService(AppDbContext context, PeakTimeService peakTimeService)
        {
            _context = context;
            _peakTimeService = peakTimeService;
        }

        public async Task<SelectSlotViewModel> BuildSlotViewModelAsync(
            int branchId, int serviceTypeId, DateTime selectedDate)
        {
            var branch = await _context.Branches
                .Include(b => b.Schedules)
                .FirstOrDefaultAsync(b => b.Id == branchId);

            var serviceType = await _context.ServiceTypes.FindAsync(serviceTypeId);

            if (branch == null || serviceType == null)
                throw new InvalidOperationException("الفرع أو الخدمة غير موجودة");

            var viewModel = new SelectSlotViewModel
            {
                BranchId = branchId,
                ServiceTypeId = serviceTypeId,
                SelectedDate = selectedDate,
                BranchName = branch.Name,
                ServiceName = serviceType.Name,
                RequiredDocuments = serviceType.RequiredDocuments
            };

            // 1. هل الفرع إجازة في اليوم ده؟
            var holiday = await _context.BranchHolidays
                .FirstOrDefaultAsync(h => h.BranchId == branchId
                    && h.Date.Date == selectedDate.Date);

            if (holiday != null)
            {
                viewModel.IsAvailable = false;
                viewModel.UnavailabilityMessage = $"الفرع إجازة النهارده - {holiday.Reason}";
                return viewModel;
            }

            // 2. هل الخدمة معطلة في الفرع ده اليوم ده؟
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

            // 3. هل الفرع بيشتغل في اليوم ده أصلاً؟
            var schedule = branch.Schedules
                .FirstOrDefault(s => s.DayOfWeek == selectedDate.DayOfWeek);

            if (schedule == null)
            {
                viewModel.IsAvailable = false;
                viewModel.UnavailabilityMessage = "الفرع مش بيشتغل في اليوم ده";
                return viewModel;
            }

            // 4. احسب الـ Slots
            var bookedAppointments = await _context.Appointments
                .Where(a => a.BranchId == branchId
                    && a.AppointmentDate.Date == selectedDate.Date
                    && a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();

            var (peakStart, peakEnd) = await _peakTimeService.GetPeakTimeAsync(branchId);
            if (peakStart == TimeSpan.Zero && peakEnd == TimeSpan.Zero)
            {
                peakStart = schedule.PeakStartTime;
                peakEnd = schedule.PeakEndTime;
            }

            viewModel.Slots = GenerateSlots(schedule, serviceType.DurationMinutes,
                bookedAppointments, peakStart, peakEnd);

            // 5. إحصائيات الازدحام
            var busiestDay = await _peakTimeService.GetBusiestDayAsync(branchId);
            viewModel.BusiestDayName = busiestDay.HasValue ? DayNames[busiestDay.Value] : null;
            viewModel.BusiestPeriod = await _peakTimeService.GetBusiestWeekPeriodAsync(branchId);

            return viewModel;
        }

        private List<SlotItemViewModel> GenerateSlots(BranchSchedule schedule, int durationMinutes,
            List<Appointment> bookedAppointments, TimeSpan peakStart, TimeSpan peakEnd)
        {
            var slots = new List<SlotItemViewModel>();
            var current = schedule.OpenTime;

            while (current < schedule.CloseTime)
            {
                var bookedCount = bookedAppointments.Count(a => a.TimeSlot == current);

                slots.Add(new SlotItemViewModel
                {
                    Time = current,
                    BookedCount = bookedCount,
                    MaxCount = schedule.MaxAppointmentsPerSlot,
                    IsBooked = bookedCount >= schedule.MaxAppointmentsPerSlot,
                    IsPeak = current >= peakStart && current < peakEnd
                });

                current = current.Add(TimeSpan.FromMinutes(durationMinutes));
            }

            return slots;
        }
    }
}
