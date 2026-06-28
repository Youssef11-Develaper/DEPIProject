using TelecomBranches.Models;

namespace TelecomBranches.ViewModels;

public class BranchIndexViewModel
{
    public List<BranchCardViewModel>  Branches     { get; set; } = new();
    public List<OperatorViewModel>    Operators    { get; set; } = new();
    public List<GovViewModel>         Governorates { get; set; } = new();
    public BranchFilterViewModel      Filter       { get; set; } = new();
    public int                        TotalCount   { get; set; }
}

public class BranchFilterViewModel
{
    public string? OperatorKey   { get; set; }
    public string? SearchQuery   { get; set; }
    public string? Status        { get; set; }
    public string? ServiceKey    { get; set; }
    public int?    GovernorateId { get; set; }
    public int?    DistrictId    { get; set; }
}

public class BranchCardViewModel
{
    public int          Id          { get; set; }
    public string       NameAr      { get; set; } = string.Empty;
    public string       ShortName   { get; set; } = string.Empty;
    public string       Area        { get; set; } = string.Empty;
    public string       Address     { get; set; } = string.Empty;
    public double       DistanceKm  { get; set; }
    public BranchStatus Status      { get; set; }
    public string       StatusLabel { get; set; } = string.Empty;
    public int          QueueCount  { get; set; }
    public string       WaitTime    { get; set; } = string.Empty;
    public double       Rating      { get; set; }

    public OperatorViewModel      Operator    { get; set; } = new();
    public GovViewModel           Governorate { get; set; } = new();
    public DistrictViewModel      District    { get; set; } = new();
    public List<ServiceItemViewModel> Services { get; set; } = new();
}

public class OperatorViewModel
{
    public int    Id      { get; set; }
    public string Key     { get; set; } = string.Empty;
    public string NameAr  { get; set; } = string.Empty;
    public string Color   { get; set; } = string.Empty;
    public string BgColor { get; set; } = string.Empty;
    public string Emoji   { get; set; } = string.Empty;
    public string Hotline { get; set; } = string.Empty;
}

public class GovViewModel
{
    public int    Id         { get; set; }
    public string NameAr     { get; set; } = string.Empty;
    public string Region     { get; set; } = string.Empty;
    public string Emoji      { get; set; } = string.Empty;
    public int    BranchCount{ get; set; }
    public List<DistrictViewModel> Districts { get; set; } = new();
}

public class DistrictViewModel
{
    public int    Id         { get; set; }
    public string NameAr     { get; set; } = string.Empty;
    public string Type       { get; set; } = string.Empty;
    public int    BranchCount{ get; set; }
}

public class BranchDetailViewModel : BranchCardViewModel
{
    public List<ServiceItemViewModel>                          AllServices        { get; set; } = new();
    public Dictionary<string, List<ServiceDocumentViewModel>> DocumentsByService { get; set; } = new();
}

public class ServiceItemViewModel
{
    public string Key           { get; set; } = string.Empty;
    public string Icon          { get; set; } = string.Empty;
    public string NameAr        { get; set; } = string.Empty;
    public string EstimatedTime { get; set; } = string.Empty;
}

public class ServiceDocumentViewModel
{
    public DocType DocType { get; set; }
    public string  TextAr  { get; set; } = string.Empty;
    public string? NoteAr  { get; set; }
}

public class AppointmentCreateViewModel
{
    public int      BranchId            { get; set; }
    public string   BranchName          { get; set; } = string.Empty;
    public string   OperatorName        { get; set; } = string.Empty;
    public string   ServiceKey          { get; set; } = string.Empty;
    public string   CustomerName        { get; set; } = string.Empty;
    public string   CustomerPhone       { get; set; } = string.Empty;
    public DateTime AppointmentDate     { get; set; } = DateTime.Today.AddDays(1);
    public string   AppointmentTimeSlot { get; set; } = string.Empty;
    public string?  Notes               { get; set; }
    public List<ServiceItemViewModel> AvailableServices { get; set; } = new();
}

public class AppointmentConfirmViewModel
{
    public int      AppointmentId   { get; set; }
    public string   BranchName      { get; set; } = string.Empty;
    public string   OperatorName    { get; set; } = string.Empty;
    public string   ServiceName     { get; set; } = string.Empty;
    public string   CustomerName    { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string   AppointmentTime { get; set; } = string.Empty;
}

public class JoinQueueViewModel
{
    public int    BranchId      { get; set; }
    public string CustomerName  { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ServiceKey    { get; set; } = string.Empty;
}

public class QueueStatusViewModel
{
    public int    EntryId              { get; set; }
    public int    BranchId             { get; set; }
    public string BranchName           { get; set; } = string.Empty;
    public int    Position             { get; set; }
    public int    TotalAhead           { get; set; }
    public int    EstimatedWaitMinutes { get; set; }
}
