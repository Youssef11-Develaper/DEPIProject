

namespace Mawidy.Application.Interfaces
{
    public interface IPdfReportService
    {
        Task<byte[]> GenerateDailyReportAsync(DateTime date, int? branchId = null);
    }
}
