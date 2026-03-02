using System.Text.Json.Serialization;

namespace Gw2PveDesktop.Models;

public class FractalMapsRoot
{
    [JsonPropertyName("DailyTier")]
    public List<List<string>> DailyTier { get; set; } = new();

    [JsonPropertyName("maps")]
    public Dictionary<string, FractalMapEntry> Maps { get; set; } = new();

    [JsonPropertyName("instabilityAssets")]
    public Dictionary<string, int> InstabilityAssets { get; set; } = new();
}

public class FractalMapEntry
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("scales")]
    public List<int> Scales { get; set; } = new();

    [JsonPropertyName("localizedNames")]
    public LocalizedNames? LocalizedNames { get; set; }
}

public class LocalizedNames
{
    [JsonPropertyName("en")]
    public string En { get; set; } = "";
}
