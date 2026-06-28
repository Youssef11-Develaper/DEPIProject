namespace TelecomBranches.Models;

public class Governorate
{
    public int    Id        { get; set; }
    public string NameAr    { get; set; } = string.Empty;
    public string NameEn    { get; set; } = string.Empty;
    public string Region    { get; set; } = string.Empty;
    public string Emoji     { get; set; } = string.Empty;
    public int    SortOrder { get; set; }

    public ICollection<District> Districts { get; set; } = new List<District>();
}

public class District
{
    public int    Id            { get; set; }
    public int    GovernorateId { get; set; }
    public string NameAr        { get; set; } = string.Empty;
    public string Type          { get; set; } = string.Empty;

    public Governorate          Governorate { get; set; } = null!;
    public ICollection<Branch>  Branches    { get; set; } = new List<Branch>();
}
