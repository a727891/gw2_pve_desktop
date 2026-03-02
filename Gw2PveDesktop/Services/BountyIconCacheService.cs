using System.IO;
using System.Net.Http;

namespace Gw2PveDesktop.Services;

/// <summary>
/// Downloads bounty boss icons from gw2dat.com and caches them in AppData.
/// </summary>
public class BountyIconCacheService
{
    private static readonly HttpClient HttpClient = new();
    private readonly string _cacheDir;

    private const string BaseUrl = "https://assets.gw2dat.com/";

    public BountyIconCacheService()
    {
        _cacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Gw2PveDesktop",
            "cache");
    }

    /// <summary>
    /// Returns the local file path for the icon (downloads to cache if not present).
    /// Returns null if assetId is null or download fails.
    /// </summary>
    public async Task<string?> GetImagePathAsync(int? assetId, CancellationToken ct = default)
    {
        if (assetId is not { } id) return null;
        var fileName = $"{id}.png";
        var localPath = Path.Combine(_cacheDir, fileName);
        if (File.Exists(localPath))
            return localPath;
        try
        {
            Directory.CreateDirectory(_cacheDir);
            var url = BaseUrl + fileName;
            var bytes = await HttpClient.GetByteArrayAsync(url, ct).ConfigureAwait(false);
            await File.WriteAllBytesAsync(localPath, bytes, ct).ConfigureAwait(false);
            return localPath;
        }
        catch
        {
            return null;
        }
    }
}
