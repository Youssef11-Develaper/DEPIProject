using Microsoft.AspNetCore.Mvc.Rendering;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;

namespace Mawidy.Application.DTOs;

// ─── Dashboard ────────────────────────────────────────────────────────────────
public class AdminDashboardViewModel
{
    public int TotalBranches       { get; set; }
    public int OpenBranches        { get; set; }
    public int BusyBranches        { get; set; }
    public int ClosedBranches      { get; set; }
    public int TotalAppointments   { get; set; }
    public int TodayAppointments   { get; set; }
    public int PendingAppointments { get; set; }
    public int TotalGovernorates   { get; set; }
    public int TotalDistricts      { get; set; }
    public int ActiveQueueEntries  { get; set; }

    public List<AdminOperatorStat>    OperatorStats      { get; set; } = new();
    public List<AdminGovStat>         TopGovernorates    { get; set; } = new();
    public List<AdminServiceStat>     ServiceStats       { get; set; } = new();
    public List<AdminDayStat>         WeeklyStats        { get; set; } = new();
    public List<AdminAppointmentRow>  RecentAppointments { get; set; } = new();
    public List<AdminBusyBranch>      BusiestBranches    { get; set; } = new();
}

public class AdminOperatorStat
{
    public string OperatorKey      { get; set; } = string.Empty;
    public string NameAr           { get; set; } = string.Empty;
    public string Color            { get; set; } = string.Empty;
    public string BgColor          { get; set; } = string.Empty;
    public string Emoji            { get; set; } = string.Empty;
    public int    BranchCount      { get; set; }
    public int    OpenCount        { get; set; }
    public int    AppointmentCount { get; set; }
    public double AvgRating        { get; set; }
}

public class AdminGovStat
{
    public int    Id               { get; set; }
    public string NameAr           { get; set; } = string.Empty;
    public string Emoji            { get; set; } = string.Empty;
    public string Region           { get; set; } = string.Empty;
    public int    BranchCount      { get; set; }
    public int    DistrictCount    { get; set; }
    public int    AppointmentCount { get; set; }
}

public class AdminServiceStat
{
    public string ServiceKey { get; set; } = string.Empty;
    public string NameAr     { get; set; } = string.Empty;
    public string Icon       { get; set; } = string.Empty;
    public int    Count      { get; set; }
    public double Percentage { get; set; }
}

public class AdminDayStat
{
    public string DayAr    { get; set; } = string.Empty;
    public int    Count    { get; set; }
    public int    MaxCount { get; set; }
}

public class AdminAppointmentRow
{
    public int      Id            { get; set; }
    public string   CustomerName  { get; set; } = string.Empty;
    public string   CustomerPhone { get; set; } = string.Empty;
    public string   BranchName    { get; set; } = string.Empty;
    public string   OperatorKey   { get; set; } = string.Empty;
    public string   OperatorEmoji { get; set; } = string.Empty;
    public string   OperatorColor { get; set; } = string.Empty;
    public string   OperatorBg    { get; set; } = string.Empty;
    public string   ServiceIcon   { get; set; } = string.Empty;
    public string   ServiceName   { get; set; } = string.Empty;
    public DateTime Date          { get; set; }
    public string   Time          { get; set; } = string.Empty;
    public string   StatusLabel   { get; set; } = string.Empty;
    public string   StatusClass   { get; set; } = string.Empty;
}

public class AdminBusyBranch
{
    public int    Id            { get; set; }
    public string NameAr        { get; set; } = string.Empty;
    public string OperatorKey   { get; set; } = string.Empty;
    public string OperatorEmoji { get; set; } = string.Empty;
    public string OperatorColor { get; set; } = string.Empty;
    public string OperatorBg    { get; set; } = string.Empty;
    public string GovName       { get; set; } = string.Empty;
    public string DistName      { get; set; } = string.Empty;
    public int    QueueCount    { get; set; }
    public string WaitTime      { get; set; } = string.Empty;
    public string Status        { get; set; } = string.Empty;
}

// ─── Branch CRUD ──────────────────────────────────────────────────────────────
public class AdminBranchListViewModel
{
    public List<AdminBranchRow> Branches     { get; set; } = new();
    public int                  TotalCount   { get; set; }
    public int                  Page         { get; set; } = 1;
    public int                  PageSize     { get; set; } = 15;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public string?              SearchQuery  { get; set; }
    public string?              FilterOp     { get; set; }
    public int?                 FilterGov    { get; set; }
    public string?              FilterStatus { get; set; }
    public List<SelectListItem> Operators    { get; set; } = new();
    public List<SelectListItem> Governorates { get; set; } = new();
}

public class AdminBranchRow
{
    public int    Id               { get; set; }
    public string NameAr           { get; set; } = string.Empty;
    public string OperatorKey      { get; set; } = string.Empty;
    public string OperatorName     { get; set; } = string.Empty;
    public string OperatorEmoji    { get; set; } = string.Empty;
    public string OperatorColor    { get; set; } = string.Empty;
    public string OperatorBg       { get; set; } = string.Empty;
    public string GovName          { get; set; } = string.Empty;
    public string DistName         { get; set; } = string.Empty;
    public string Address          { get; set; } = string.Empty;
    public string Status           { get; set; } = string.Empty;
    public string StatusLabel      { get; set; } = string.Empty;
    public int    QueueCount       { get; set; }
    public string WaitTime         { get; set; } = string.Empty;
    public double Rating           { get; set; }
    public int    AppointmentCount { get; set; }
}

public class AdminBranchEditViewModel
{
    public int    Id            { get; set; }
    public int    OperatorId    { get; set; }
    public int    GovernorateId { get; set; }
    public int    DistrictId    { get; set; }
    public string NameAr        { get; set; } = string.Empty;
    public string Area          { get; set; } = string.Empty;
    public string Address       { get; set; } = string.Empty;
    public double DistanceKm    { get; set; }
    public int    Status        { get; set; }
    public int    QueueCount    { get; set; }
    public string WaitTime      { get; set; } = string.Empty;
    public double Rating        { get; set; } = 4.0;

    public List<SelectListItem> Operators    { get; set; } = new();
    public List<SelectListItem> Governorates { get; set; } = new();
    public List<SelectListItem> Districts    { get; set; } = new();
}

// ─── Appointments ─────────────────────────────────────────────────────────────
public class AdminAppointmentListViewModel
{
    public List<AdminAppointmentRow> Appointments  { get; set; } = new();
    public int                       TotalCount    { get; set; }
    public int                       Page          { get; set; } = 1;
    public int                       PageSize      { get; set; } = 20;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public string?              FilterStatus { get; set; }
    public string?              FilterOp     { get; set; }
    public string?              FilterDate   { get; set; }
    public List<SelectListItem> Operators    { get; set; } = new();
}

// ─── Reports ──────────────────────────────────────────────────────────────────
public class AdminReportsViewModel
{
    public string                  Period       { get; set; } = "month";
    public int                     TotalAppts   { get; set; }
    public int                     TotalBranches{ get; set; }
    public List<AdminOperatorStat> OperatorStats{ get; set; } = new();
    public List<AdminGovStat>      GovStats     { get; set; } = new();
    public List<AdminServiceStat>  ServiceStats { get; set; } = new();
    public List<AdminMonthlyStat>  MonthlyTrend { get; set; } = new();
}

public class AdminMonthlyStat
{
    public string MonthLabel { get; set; } = string.Empty;
    public int    Count      { get; set; }
}
