using System.Text.Json.Serialization;

namespace Gw2PveDesktop.Models;

/// <summary>
/// Minimal model for raid_data.json (expansions.wings.encounters) to resolve encounter api_id to display name.
/// </summary>
public class RaidDataRoot
{
    [JsonPropertyName("expansions")]
    public List<RaidExpansion> Expansions { get; set; } = new();
}

public class RaidExpansion
{
    [JsonPropertyName("wings")]
    public List<RaidWing> Wings { get; set; } = new();
}

public class RaidWing
{
    [JsonPropertyName("encounters")]
    public List<RaidEncounter> Encounters { get; set; } = new();
}

public class RaidEncounter
{
    [JsonPropertyName("api_id")]
    public string ApiId { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("assetId")]
    public int AssetId { get; set; }

    [JsonPropertyName("localizedNames")]
    public RaidLocalizedNames? LocalizedNames { get; set; }
}

public class RaidLocalizedNames
{
    [JsonPropertyName("en")]
    public string? En { get; set; }
}
