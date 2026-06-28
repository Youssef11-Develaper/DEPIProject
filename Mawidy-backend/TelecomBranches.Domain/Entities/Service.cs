using TelecomBranches.Domain.Enums;

namespace TelecomBranches.Domain.Entities;

public class OperatorService
{
    public int    Id            { get; set; }
    public int    OperatorId    { get; set; }
    public string ServiceKey    { get; set; } = string.Empty;
    public string Icon          { get; set; } = string.Empty;
    public string NameAr        { get; set; } = string.Empty;
    public string EstimatedTime { get; set; } = string.Empty;

    public Operator Operator { get; set; } = null!;
}

public class ServiceDocument
{
    public int     Id         { get; set; }
    public string  ServiceKey { get; set; } = string.Empty;
    public DocType DocType    { get; set; }
    public string  TextAr     { get; set; } = string.Empty;
    public string? NoteAr     { get; set; }
    public int     SortOrder  { get; set; }
}
