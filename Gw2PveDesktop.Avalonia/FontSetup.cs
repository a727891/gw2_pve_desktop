using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Platform;

namespace Gw2PveDesktop.Avalonia;

internal static class FontSetup
{
    private const string AssetsUri = "avares://Gw2PveDesktop/Assets";
    private const string FontResourceUri = "avares://Gw2PveDesktop/Assets/menomonia.ttf";
    private const string FontFamilyName = "Menomonia";

    public static FontFamily GuildWarsFont { get; } = CreateGuildWarsFont();

    private static FontFamily CreateGuildWarsFont()
    {
        try
        {
            var assets = new Uri(AssetsUri);
            FontManager.Current.AddFontCollection(new EmbeddedFontCollection(assets, assets));
            return new FontFamily(FontFamilyName);
        }
        catch
        {
            // Fall through to file-based load.
        }

        try
        {
            using var stream = AssetLoader.Open(new Uri(FontResourceUri));
            var fontPath = Path.Combine(PlatformPaths.IconCacheDirectory, "menomonia.ttf");
            Directory.CreateDirectory(PlatformPaths.IconCacheDirectory);
            using (var file = File.Create(fontPath))
                stream.CopyTo(file);

            return new FontFamily(fontPath);
        }
        catch
        {
            return new FontFamily("Consolas");
        }
    }
}
