using Microsoft.AspNetCore.Mvc.Rendering;
using TelecomBranches.Application.DTOs;

namespace TelecomBranches.Application.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardViewModel>       GetDashboardAsync();
    Task<AdminBranchListViewModel>      GetBranchesAsync(AdminBranchListViewModel filter);
    Task<AdminBranchEditViewModel>      GetBranchEditAsync(int id = 0);
    Task<int>                           SaveBranchAsync(AdminBranchEditViewModel vm);
    Task                                DeleteBranchAsync(int id);
    Task<AdminAppointmentListViewModel> GetAppointmentsAsync(AdminAppointmentListViewModel filter);
    Task                                UpdateAppointmentStatusAsync(int id, string status);
    Task<AdminReportsViewModel>         GetReportsAsync(string period);
    Task<List<SelectListItem>>          GetDistrictsSelectAsync(int governorateId);
}
