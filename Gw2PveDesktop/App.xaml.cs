using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Gw2PveDesktop.Services;
using Application = System.Windows.Application;

namespace Gw2PveDesktop;

internal static class NativeMethods
{
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyIcon(IntPtr hIcon);
}

public partial class App : Application
{
    private static readonly Mutex SingleInstanceMutex = new(false, "Gw2PveDesktop_SingleInstance");
    private NotifyIcon? _notifyIcon;
    private System.Drawing.Icon? _trayIcon;
    private PopupWindow? _popup;
    private readonly DataService _dataService;
    private readonly ScheduleService _scheduleService;
    private readonly BountyIconCacheService _iconCache;

    public const string StaticHostUrl = "https://bhm.blishhud.com/Soeed.RaidClears/static/";
    public const string StaticHostApiVersion = "v2/";
    public static string BaseUrl => StaticHostUrl + StaticHostApiVersion;

    public App()
    {
        _dataService = new DataService(BaseUrl);
        _scheduleService = new ScheduleService();
        _iconCache = new BountyIconCacheService();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        if (!SingleInstanceMutex.WaitOne(0, false))
        {
            System.Windows.MessageBox.Show("GW2 PvE is already running.", "GW2 PvE");
            Shutdown();
            return;
        }

        _notifyIcon = new NotifyIcon
        {
            Text = "GW2 PvE",
            Visible = true
        };

        try
        {
            _trayIcon = LoadTrayIconFromEmbeddedPng();
            if (_trayIcon != null)
                _notifyIcon.Icon = _trayIcon;
            else
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
        }
        catch
        {
            _notifyIcon.Icon ??= System.Drawing.SystemIcons.Application;
        }

        var showItem = new ToolStripMenuItem("Show");
        showItem.Click += (_, _) => ShowPopup();

        var refreshItem = new ToolStripMenuItem("Refresh");
        refreshItem.Click += async (_, _) => await RefreshDataAsync();

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => Shutdown();

        _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
        _notifyIcon.ContextMenuStrip.Items.Add(showItem);
        _notifyIcon.ContextMenuStrip.Items.Add(refreshItem);
        _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _notifyIcon.ContextMenuStrip.Items.Add(exitItem);

        _notifyIcon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                TogglePopup();
        };

        _ = RefreshDataAsync();
    }

    /// <summary>Load tray icon from embedded PNG (preserves transparency; avoids using exe .ico).</summary>
    private static System.Drawing.Icon? LoadTrayIconFromEmbeddedPng()
    {
        var asm = System.Reflection.Assembly.GetExecutingAssembly();
        var name = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(".1128644.png", StringComparison.OrdinalIgnoreCase));
        if (name == null) return null;
        using var stream = asm.GetManifestResourceStream(name);
        if (stream == null) return null;
        using var bmp = new System.Drawing.Bitmap(stream);
        var hIcon = bmp.GetHicon();
        try
        {
            return (System.Drawing.Icon)System.Drawing.Icon.FromHandle(hIcon).Clone();
        }
        finally
        {
            _ = NativeMethods.DestroyIcon(hIcon);
        }
    }

    private void TogglePopup()
    {
        if (_popup == null)
        {
            _popup = new PopupWindow(_scheduleService, _iconCache);
            _popup.Closed += (_, _) => _popup = null;
        }

        if (_popup.IsVisible)
        {
            _popup.Hide();
        }
        else
        {
            ShowPopup();
        }
    }

    private void ShowPopup()
    {
        if (_popup == null)
        {
            _popup = new PopupWindow(_scheduleService, _iconCache);
            _popup.Closed += (_, _) => _popup = null;
        }

        _popup.RefreshData();
        _popup.Show();
        _popup.Activate();

        // Position near tray (bottom-right)
        var workingArea = System.Windows.Forms.Screen.PrimaryScreen?.WorkingArea ?? new System.Drawing.Rectangle(0, 0, 400, 300);
        _popup.Left = workingArea.Right - _popup.Width - 10;
        _popup.Top = workingArea.Bottom - _popup.Height - 10;
    }

    private async Task RefreshDataAsync()
    {
        try
        {
            var maps = await _dataService.GetFractalMapsAsync();
            var instabilities = await _dataService.GetFractalInstabilitiesAsync();
            var bounties = await _dataService.GetDailyBountiesAsync();
            var raidData = await _dataService.GetRaidDataAsync();
            var strikeData = await _dataService.GetStrikeDataAsync();
            _scheduleService.LoadData(maps, instabilities, bounties, raidData, strikeData);
            _popup?.RefreshData();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to load data: {ex.Message}", "GW2 PvE");
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _notifyIcon?.Dispose();
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
