namespace Mawidy.Domain.Entities;

public class Operator
{
    public int    Id      { get; set; }
    public string Key     { get; set; } = string.Empty;
    public string NameAr  { get; set; } = string.Empty;
    public string Color   { get; set; } = string.Empty;
    public string BgColor { get; set; } = string.Empty;
    public string Emoji   { get; set; } = string.Empty;
    public string Hotline { get; set; } = string.Empty;

    public ICollection<Branch>          Branches { get; set; } = new List<Branch>();
    public ICollection<OperatorService> Services { get; set; } = new List<OperatorService>();
}
