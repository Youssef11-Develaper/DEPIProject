using Mawidy.Application.DTOs;

namespace Mawidy.Application.Interfaces;

public interface IBranchService
{
    Task<BranchIndexViewModel>           GetBranchListAsync(BranchFilterViewModel filter);
    Task<BranchDetailViewModel?>         GetBranchDetailAsync(int id);
    Task<List<OperatorViewModel>>        GetOperatorsAsync();
    Task<List<GovViewModel>>             GetGovernoratesAsync();
    Task<List<DistrictViewModel>>        GetDistrictsByGovernorateAsync(int governorateId);
    Task<List<ServiceDocumentViewModel>> GetServiceDocumentsAsync(string serviceKey);
}
