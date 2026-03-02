using System.Text.Json.Serialization;

namespace Gw2PveDesktop.Models;

/// <summary>
/// Minimal model for strike_data.json (expansions.missions) to resolve strike encounter id to display name and assetId.
/// </summary>
public class StrikeDataRoot
{
    [JsonPropertyName("expansions")]
    public List<StrikeExpansion> Expansions { get; set; } = new();
}

public class StrikeExpansion
{
    [JsonPropertyName("missions")]
    public List<StrikeMission> Missions { get; set; } = new();
}

public class StrikeMission
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("assetId")]
    public int AssetId { get; set; }

    [JsonPropertyName("localizedNames")]
    public StrikeLocalizedNames? LocalizedNames { get; set; }
}

public class StrikeLocalizedNames
{
    [JsonPropertyName("en")]
    public string? En { get; set; }
}
