

namespace Mawidy.Application.Interfaces
{
    public interface IPeakTimeService
    {
        Task<(TimeSpan PeakStart, TimeSpan PeakEnd)> GetPeakTimeAsync(int branchId);
        Task<DayOfWeek?> GetBusiestDayAsync(int branchId);
        Task<string?> GetBusiestWeekPeriodAsync(int branchId);
        string GetDayName(DayOfWeek day);
    }
}
