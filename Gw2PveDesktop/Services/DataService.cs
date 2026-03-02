using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Gw2PveDesktop.Models;

namespace Gw2PveDesktop.Services;

public class DataService
{
    private readonly HttpClient _http = new();
    private readonly string _baseUrl;

    public DataService(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/') + "/";
    }

    public async Task<FractalMapsRoot?> GetFractalMapsAsync(CancellationToken ct = default)
    {
        var url = _baseUrl + "fractal_maps.json";
        return await _http.GetFromJsonAsync<FractalMapsRoot>(url, ct);
    }

    public async Task<FractalInstabilitiesRoot?> GetFractalInstabilitiesAsync(CancellationToken ct = default)
    {
        var url = _baseUrl + "fractal_instabilities.json";
        return await _http.GetFromJsonAsync<FractalInstabilitiesRoot>(url, ct);
    }

    public async Task<DailyBountiesRoot?> GetDailyBountiesAsync(CancellationToken ct = default)
    {
        var url = _baseUrl + "daily_bounties.json";
        return await _http.GetFromJsonAsync<DailyBountiesRoot>(url, ct);
    }

    public async Task<RaidDataRoot?> GetRaidDataAsync(CancellationToken ct = default)
    {
        var url = _baseUrl + "raid_data.json";
        return await _http.GetFromJsonAsync<RaidDataRoot>(url, ct);
    }

    public async Task<StrikeDataRoot?> GetStrikeDataAsync(CancellationToken ct = default)
    {
        var url = _baseUrl + "strike_data.json";
        return await _http.GetFromJsonAsync<StrikeDataRoot>(url, ct);
    }
}
