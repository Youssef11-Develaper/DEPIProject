using Microsoft.EntityFrameworkCore;
using TelecomBranches.Data;
using TelecomBranches.Models;
using TelecomBranches.ViewModels;

namespace TelecomBranches.Services;

public interface IBranchService
{
    Task<BranchIndexViewModel>           GetBranchListAsync(BranchFilterViewModel filter);
    Task<BranchDetailViewModel?>         GetBranchDetailAsync(int id);
    Task<List<OperatorViewModel>>        GetOperatorsAsync();
    Task<List<GovViewModel>>             GetGovernoratesAsync();
    Task<List<DistrictViewModel>>        GetDistrictsByGovernorateAsync(int governorateId);
    Task<List<ServiceDocumentViewModel>> GetServiceDocumentsAsync(string serviceKey);
}

public class BranchService : IBranchService
{
    private readonly AppDbContext _db;
    public BranchService(AppDbContext db) => _db = db;

    public async Task<BranchIndexViewModel> GetBranchListAsync(BranchFilterViewModel filter)
    {
        var query = _db.Branches
            .Include(b => b.Operator).ThenInclude(o => o.Services)
            .Include(b => b.Governorate)
            .Include(b => b.District)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.OperatorKey) && filter.OperatorKey != "all")
            query = query.Where(b => b.Operator.Key == filter.OperatorKey);

        if (filter.GovernorateId.HasValue)
            query = query.Where(b => b.GovernorateId == filter.GovernorateId.Value);

        if (filter.DistrictId.HasValue)
            query = query.Where(b => b.DistrictId == filter.DistrictId.Value);

        if (!string.IsNullOrEmpty(filter.Status))
        {
            var st = filter.Status switch
            {
                "open"   => BranchStatus.Open,
                "busy"   => BranchStatus.Busy,
                "closed" => BranchStatus.Closed,
                _        => (BranchStatus?)null
            };
            if (st.HasValue) query = query.Where(b => b.Status == st.Value);
        }

        if (!string.IsNullOrEmpty(filter.SearchQuery))
        {
            var q = filter.SearchQuery.Trim();
            query = query.Where(b =>
                b.NameAr.Contains(q) ||
                b.Area.Contains(q) ||
                b.Address.Contains(q) ||
                b.Governorate.NameAr.Contains(q) ||
                b.District.NameAr.Contains(q));
        }

        if (!string.IsNullOrEmpty(filter.ServiceKey))
        {
            var sk = filter.ServiceKey;
            query = query.Where(b => b.Operator.Services.Any(s => s.ServiceKey == sk));
        }

        var total    = await _db.Branches.CountAsync();
        var branches = await query.OrderBy(b => b.GovernorateId).ThenBy(b => b.DistanceKm).ToListAsync();

        return new BranchIndexViewModel
        {
            Branches     = branches.Select(MapToCard).ToList(),
            Operators    = await GetOperatorsAsync(),
            Governorates = await GetGovernoratesAsync(),
            Filter       = filter,
            TotalCount   = total
        };
    }

    public async Task<BranchDetailViewModel?> GetBranchDetailAsync(int id)
    {
        var b = await _db.Branches
            .Include(b => b.Operator).ThenInclude(o => o.Services)
            .Include(b => b.Governorate)
            .Include(b => b.District)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (b is null) return null;

        var allDocs = await _db.ServiceDocuments
            .Where(d => b.Operator.Services.Select(s => s.ServiceKey).Contains(d.ServiceKey))
            .OrderBy(d => d.SortOrder)
            .ToListAsync();

        var docsByService = allDocs
            .GroupBy(d => d.ServiceKey)
            .ToDictionary(
                g => g.Key,
                g => g.Select(d => new ServiceDocumentViewModel
                {
                    DocType = d.DocType,
                    TextAr  = d.TextAr,
                    NoteAr  = d.NoteAr
                }).ToList());

        var card = MapToCard(b);
        return new BranchDetailViewModel
        {
            Id          = card.Id,       NameAr      = card.NameAr,
            ShortName   = card.ShortName,Area        = card.Area,
            Address     = card.Address,  DistanceKm  = card.DistanceKm,
            Status      = card.Status,   StatusLabel = card.StatusLabel,
            QueueCount  = card.QueueCount,WaitTime   = card.WaitTime,
            Rating      = card.Rating,   Operator    = card.Operator,
            Governorate = card.Governorate,District  = card.District,
            Services    = card.Services,
            AllServices = b.Operator.Services.Select(s => new ServiceItemViewModel
            {
                Key = s.ServiceKey, Icon = s.Icon, NameAr = s.NameAr, EstimatedTime = s.EstimatedTime
            }).ToList(),
            DocumentsByService = docsByService
        };
    }

    public async Task<List<OperatorViewModel>> GetOperatorsAsync() =>
        await _db.Operators.Select(o => new OperatorViewModel
        {
            Id = o.Id, Key = o.Key, NameAr = o.NameAr,
            Color = o.Color, BgColor = o.BgColor, Emoji = o.Emoji, Hotline = o.Hotline
        }).ToListAsync();

    public async Task<List<GovViewModel>> GetGovernoratesAsync()
    {
        var govs = await _db.Governorates
            .Include(g => g.Districts).ThenInclude(d => d.Branches)
            .OrderBy(g => g.SortOrder)
            .ToListAsync();

        return govs.Select(g => new GovViewModel
        {
            Id      = g.Id,
            NameAr  = g.NameAr,
            Region  = g.Region,
            Emoji   = g.Emoji,
            BranchCount = g.Districts.SelectMany(d => d.Branches).Count(),
            Districts   = g.Districts.Select(d => new DistrictViewModel
            {
                Id = d.Id, NameAr = d.NameAr, Type = d.Type, BranchCount = d.Branches.Count
            }).OrderBy(d => d.NameAr).ToList()
        }).ToList();
    }

    public async Task<List<DistrictViewModel>> GetDistrictsByGovernorateAsync(int governorateId) =>
        await _db.Districts
            .Where(d => d.GovernorateId == governorateId)
            .OrderBy(d => d.NameAr)
            .Select(d => new DistrictViewModel
            {
                Id = d.Id, NameAr = d.NameAr, Type = d.Type, BranchCount = d.Branches.Count()
            }).ToListAsync();

    public async Task<List<ServiceDocumentViewModel>> GetServiceDocumentsAsync(string serviceKey) =>
        await _db.ServiceDocuments
            .Where(d => d.ServiceKey == serviceKey)
            .OrderBy(d => d.SortOrder)
            .Select(d => new ServiceDocumentViewModel
            {
                DocType = d.DocType, TextAr = d.TextAr, NoteAr = d.NoteAr
            }).ToListAsync();

    private static BranchCardViewModel MapToCard(Branch b)
    {
        var label = b.Status switch
        {
            BranchStatus.Open   => "متاح",
            BranchStatus.Busy   => "مزدحم",
            BranchStatus.Closed => "مغلق",
            _                   => ""
        };
        var short_ = b.NameAr.Contains(" - ")
            ? b.NameAr[(b.NameAr.IndexOf(" - ", StringComparison.Ordinal) + 3)..].Trim()
            : b.NameAr;

        return new BranchCardViewModel
        {
            Id          = b.Id,
            NameAr      = b.NameAr,
            ShortName   = short_,
            Area        = b.Area,
            Address     = b.Address,
            DistanceKm  = b.DistanceKm,
            Status      = b.Status,
            StatusLabel = label,
            QueueCount  = b.QueueCount,
            WaitTime    = b.WaitTime,
            Rating      = b.Rating,
            Operator    = new OperatorViewModel
            {
                Id = b.Operator.Id, Key = b.Operator.Key, NameAr = b.Operator.NameAr,
                Color = b.Operator.Color, BgColor = b.Operator.BgColor,
                Emoji = b.Operator.Emoji, Hotline = b.Operator.Hotline
            },
            Governorate = new GovViewModel
            {
                Id = b.Governorate.Id, NameAr = b.Governorate.NameAr,
                Region = b.Governorate.Region, Emoji = b.Governorate.Emoji
            },
            District = new DistrictViewModel
            {
                Id = b.District.Id, NameAr = b.District.NameAr, Type = b.District.Type
            },
            Services = b.Operator.Services.Take(3).Select(s => new ServiceItemViewModel
            {
                Key = s.ServiceKey, Icon = s.Icon, NameAr = s.NameAr, EstimatedTime = s.EstimatedTime
            }).ToList()
        };
    }
}
