namespace Gw2PveDesktop;

public static class PlatformPaths
{
    /// <summary>
    /// Directory for cached gw2dat.com icon PNGs.
    /// Windows: %LOCALAPPDATA%/Gw2PveDesktop/cache
    /// Linux: $XDG_CACHE_HOME/gw2-pve-desktop (default ~/.cache/gw2-pve-desktop)
    /// </summary>
    public static string IconCacheDirectory
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Gw2PveDesktop",
                    "cache");
            }

            var xdgCache = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
            if (string.IsNullOrEmpty(xdgCache))
            {
                xdgCache = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".cache");
            }

            return Path.Combine(xdgCache, "gw2-pve-desktop");
        }
    }
}
