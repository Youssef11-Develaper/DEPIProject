using System.Text.Json.Serialization;

namespace Mawidy.Web.ViewModels.Courts;

public class FaqViewModel
{
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("q")]
    public string Q { get; set; } = string.Empty;

    [JsonPropertyName("a")]
    public string A { get; set; } = string.Empty;
}
