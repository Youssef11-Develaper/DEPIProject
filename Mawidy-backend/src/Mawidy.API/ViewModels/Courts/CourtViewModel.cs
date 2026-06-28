using System.Text.Json.Serialization;

namespace Mawidy.API.ViewModels.Courts;

public class CourtViewModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("addr")]
    public string Addr { get; set; } = string.Empty;

    [JsonPropertyName("dist")]
    public string Dist { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("queue")]
    public int Queue { get; set; }

    [JsonPropertyName("wait")]
    public string Wait { get; set; } = string.Empty;

    [JsonPropertyName("rooms")]
    public int Rooms { get; set; }

    [JsonPropertyName("sessions")]
    public int Sessions { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;
}
