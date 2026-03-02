using Gw2PveDesktop.Models;

namespace Gw2PveDesktop.Services;

public class ScheduleService
{
    private FractalMapsRoot? _fractalMaps;
    private FractalInstabilitiesRoot? _instabilities;
    private DailyBountiesRoot? _bounties;
    private Dictionary<string, string> _encounterDisplayNames = new(StringComparer.OrdinalIgnoreCase);

    public void LoadData(FractalMapsRoot? fractalMaps, FractalInstabilitiesRoot? instabilities, DailyBountiesRoot? bounties, RaidDataRoot? raidData = null)
    {
        _fractalMaps = fractalMaps;
        _instabilities = instabilities;
        _bounties = bounties;
        _encounterDisplayNames = BuildEncounterDisplayNames(raidData);
    }

    private static Dictionary<string, string> BuildEncounterDisplayNames(RaidDataRoot? raidData)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (raidData?.Expansions == null) return map;
        foreach (var expansion in raidData.Expansions)
        {
            if (expansion.Wings == null) continue;
            foreach (var wing in expansion.Wings)
            {
                if (wing.Encounters == null) continue;
                foreach (var enc in wing.Encounters)
                {
                    if (string.IsNullOrEmpty(enc.ApiId)) continue;
                    var displayName = enc.LocalizedNames?.En ?? enc.Name ?? enc.ApiId;
                    if (!string.IsNullOrEmpty(displayName))
                        map[enc.ApiId] = displayName;
                }
            }
        }
        return map;
    }

    public ScheduleViewModel GetSchedule()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return new ScheduleViewModel
        {
            FractalsToday = GetFractalsForDate(today),
            FractalsTomorrow = GetFractalsForDate(tomorrow),
            BountiesToday = GetBountiesForDate(today),
            BountiesTomorrow = GetBountiesForDate(tomorrow)
        };
    }

    private FractalDayViewModel GetFractalsForDate(DateTime date)
    {
        var result = new FractalDayViewModel();
        if (_fractalMaps == null || _instabilities == null) return result;

        var dayIndex = DayOfYearIndexService.DayOfYearIndex(date);
        if (_fractalMaps.DailyTier.Count == 0) return result;
        var tierIndex = dayIndex % _fractalMaps.DailyTier.Count;
        var tier = _fractalMaps.DailyTier[tierIndex];
        if (tier == null) return result;

        foreach (var mapKey in tier)
        {
            if (string.IsNullOrEmpty(mapKey)) continue;
            if (!_fractalMaps.Maps.TryGetValue(mapKey, out var map)) continue;

            var label = map.LocalizedNames?.En ?? map.Label ?? mapKey;
            var t4Scale = map.Scales.Where(s => s >= 76 && s <= 100).Cast<int?>().Max();
            if (t4Scale == null) t4Scale = map.Scales.DefaultIfEmpty(0).Max();

            var instabNames = new List<string>();
            if (t4Scale.HasValue && _instabilities.Instabilities.TryGetValue(t4Scale.Value.ToString(), out var days) && dayIndex < days.Count)
            {
                var ids = days[dayIndex];
                foreach (var id in ids)
                {
                    if (id >= 0 && id < _instabilities.InstabilityNames.Count)
                        instabNames.Add(_instabilities.InstabilityNames[id]);
                }
            }

            result.Fractals.Add(new FractalEntryViewModel { Name = label, Instabilities = instabNames });
        }

        return result;
    }

    private BountyDayViewModel GetBountiesForDate(DateTime date)
    {
        var result = new BountyDayViewModel();
        if (_bounties == null) return result;

        var dayIndex = DayOfYearIndexService.DayOfYearIndex(date);
        foreach (var slot in _bounties.BossSlots.OrderBy(s => s.Slot))
        {
            if (slot.Encounters.Count == 0) continue;
            var idx = (dayIndex + slot.Offset) % slot.Encounters.Count;
            var encounterId = slot.Encounters[idx];
            result.Bounties.Add(GetBountyDisplayName(encounterId));
        }

        return result;
    }

    private string GetBountyDisplayName(string encounterId)
    {
        return _encounterDisplayNames.TryGetValue(encounterId, out var name) ? name : ToTitleCase(encounterId);
    }

    private static string ToTitleCase(string id)
    {
        if (string.IsNullOrEmpty(id)) return id;
        return string.Join(" ", id.Split('_').Select(s => s.Length > 0 ? char.ToUpperInvariant(s[0]) + s[1..].ToLowerInvariant() : s));
    }
}

public class ScheduleViewModel
{
    public FractalDayViewModel FractalsToday { get; set; } = new();
    public FractalDayViewModel FractalsTomorrow { get; set; } = new();
    public BountyDayViewModel BountiesToday { get; set; } = new();
    public BountyDayViewModel BountiesTomorrow { get; set; } = new();
}

public class FractalDayViewModel
{
    public List<FractalEntryViewModel> Fractals { get; set; } = new();
}

public class FractalEntryViewModel
{
    public string Name { get; set; } = "";
    public List<string> Instabilities { get; set; } = new();
}

public class BountyDayViewModel
{
    public List<string> Bounties { get; set; } = new();
}
