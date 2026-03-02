using System.Text.Json.Serialization;

namespace Gw2PveDesktop.Models;

public class FractalInstabilitiesRoot
{
    [JsonPropertyName("instabilities")]
    public Dictionary<string, List<List<int>>> Instabilities { get; set; } = new();

    [JsonPropertyName("instability_names")]
    public List<string> InstabilityNames { get; set; } = new();
}
