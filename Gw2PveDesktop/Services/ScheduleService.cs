using System.ComponentModel;
using System.Runtime.CompilerServices;
using Gw2PveDesktop.Models;

namespace Gw2PveDesktop.Services;

public class ScheduleService
{
    private FractalMapsRoot? _fractalMaps;
    private FractalInstabilitiesRoot? _instabilities;
    private DailyBountiesRoot? _bounties;
    private Dictionary<string, (string Name, int? AssetId)> _encounterInfo = new(StringComparer.OrdinalIgnoreCase);

    public void LoadData(FractalMapsRoot? fractalMaps, FractalInstabilitiesRoot? instabilities, DailyBountiesRoot? bounties, RaidDataRoot? raidData = null, StrikeDataRoot? strikeData = null)
    {
        _fractalMaps = fractalMaps;
        _instabilities = instabilities;
        _bounties = bounties;
        _encounterInfo = BuildEncounterInfo(raidData, strikeData);
    }

    private static Dictionary<string, (string Name, int? AssetId)> BuildEncounterInfo(RaidDataRoot? raidData, StrikeDataRoot? strikeData)
    {
        var map = new Dictionary<string, (string Name, int? AssetId)>(StringComparer.OrdinalIgnoreCase);
        if (raidData?.Expansions != null)
        {
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
                        if (string.IsNullOrEmpty(displayName)) continue;
                        var assetId = enc.AssetId != 0 ? enc.AssetId : (int?)null;
                        map[enc.ApiId] = (displayName, assetId);
                    }
                }
            }
        }
        if (strikeData?.Expansions != null)
        {
            foreach (var expansion in strikeData.Expansions)
            {
                if (expansion.Missions == null) continue;
                foreach (var mission in expansion.Missions)
                {
                    if (string.IsNullOrEmpty(mission.Id)) continue;
                    var displayName = mission.LocalizedNames?.En ?? mission.Name ?? mission.Id;
                    if (string.IsNullOrEmpty(displayName)) continue;
                    var assetId = mission.AssetId != 0 ? mission.AssetId : (int?)null;
                    map[mission.Id] = (displayName, assetId);
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

            var instabList = new List<InstabilityEntryViewModel>();
            if (t4Scale.HasValue && _instabilities.Instabilities.TryGetValue(t4Scale.Value.ToString(), out var days) && dayIndex < days.Count)
            {
                var ids = days[dayIndex];
                foreach (var id in ids)
                {
                    if (id >= 0 && id < _instabilities.InstabilityNames.Count)
                    {
                        var name = _instabilities.InstabilityNames[id];
                        var assetId = _fractalMaps.InstabilityAssets.TryGetValue(name, out var aid) ? aid : (int?)null;
                        instabList.Add(new InstabilityEntryViewModel { Name = name, AssetId = assetId });
                    }
                }
            }

            result.Fractals.Add(new FractalEntryViewModel { Name = label, Instabilities = instabList });
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
            result.Bounties.Add(GetBountyEntry(encounterId));
        }

        return result;
    }

    private BountyEntryViewModel GetBountyEntry(string encounterId)
    {
        if (_encounterInfo.TryGetValue(encounterId, out var info))
            return new BountyEntryViewModel { Name = info.Name, AssetId = info.AssetId };
        return new BountyEntryViewModel { Name = ToTitleCase(encounterId), AssetId = null };
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
    public List<InstabilityEntryViewModel> Instabilities { get; set; } = new();
}

public class InstabilityEntryViewModel : INotifyPropertyChanged
{
    private string _imagePath = "";

    public string Name { get; set; } = "";
    public int? AssetId { get; set; }

    public string ImagePath
    {
        get => _imagePath;
        set { _imagePath = value ?? ""; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class BountyDayViewModel
{
    public List<BountyEntryViewModel> Bounties { get; set; } = new();
}

public class BountyEntryViewModel : INotifyPropertyChanged
{
    private string _imagePath = "";

    public string Name { get; set; } = "";
    public int? AssetId { get; set; }

    public string ImagePath
    {
        get => _imagePath;
        set { _imagePath = value ?? ""; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
