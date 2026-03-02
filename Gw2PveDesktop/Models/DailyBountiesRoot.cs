using System.Text.Json.Serialization;

namespace Gw2PveDesktop.Models;

public class DailyBountiesRoot
{
    [JsonPropertyName("bossSlots")]
    public List<BossSlot> BossSlots { get; set; } = new();
}

public class BossSlot
{
    [JsonPropertyName("slot")]
    public int Slot { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("encounters")]
    public List<string> Encounters { get; set; } = new();
}
