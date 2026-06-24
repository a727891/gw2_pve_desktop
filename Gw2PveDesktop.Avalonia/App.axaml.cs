using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Gw2PveDesktop.Services;

namespace Gw2PveDesktop.Avalonia;

public partial class App : Application
{
    private static readonly Mutex SingleInstanceMutex = new(false, "Gw2PveDesktop_SingleInstance");
    private TrayIcon? _trayIcon;
    private PopupWindow? _popup;
    private bool _hasShownPopupOnStartup;
    private readonly DataService _dataService;
    private readonly ScheduleService _scheduleService;
    private readonly BountyIconCacheService _iconCache;

    public App()
    {
        AvaloniaXamlLoader.Load(this);
        _ = FontSetup.GuildWarsFont;
        _dataService = new DataService(AppConstants.BaseUrl);
        _scheduleService = new ScheduleService();
        _iconCache = new BountyIconCacheService();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        if (!SingleInstanceMutex.WaitOne(0, false))
        {
            ShowMessage("GW2 PvE is already running.", "GW2 PvE");
            desktop.Shutdown();
            return;
        }

        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        desktop.Exit += (_, _) =>
        {
            _trayIcon?.Dispose();
            SingleInstanceMutex.ReleaseMutex();
        };

        SetupTrayIcon();
        _ = RefreshDataAsync();

        base.OnFrameworkInitializationCompleted();
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new TrayIcon
        {
            ToolTipText = "GW2 PvE",
            IsVisible = true
        };

        try
        {
            var iconStream = AssetLoader.Open(new Uri("avares://Gw2PveDesktop/Assets/1128644.png"));
            _trayIcon.Icon = new WindowIcon(iconStream);
        }
        catch
        {
            // Tray may still work without a custom icon on some desktops.
        }

        var showItem = new NativeMenuItem("Show");
        showItem.Click += (_, _) => ShowPopup();

        var refreshItem = new NativeMenuItem("Refresh");
        refreshItem.Click += async (_, _) => await RefreshDataAsync();

        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += (_, _) =>
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
        };

        var menu = new NativeMenu();
        menu.Items.Add(showItem);
        menu.Items.Add(refreshItem);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(exitItem);
        _trayIcon.Menu = menu;

        _trayIcon.Clicked += (_, _) => TogglePopup();
    }

    private void TogglePopup()
    {
        if (_popup == null)
        {
            _popup = new PopupWindow(_scheduleService, _iconCache);
            _popup.Closed += (_, _) => _popup = null;
        }

        if (_popup.IsVisible)
            _popup.Hide();
        else
            ShowPopup();
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
            if (!_hasShownPopupOnStartup)
            {
                _hasShownPopupOnStartup = true;
                await Dispatcher.UIThread.InvokeAsync(ShowPopup);
            }
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
                ShowMessage($"Failed to load data: {ex.Message}", "GW2 PvE"));
        }
    }

    private static void ShowMessage(string message, string title)
    {
        var window = new Window
        {
            Title = title,
            Width = 420,
            Height = 140,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            CanResize = false,
            Content = new StackPanel
            {
                Margin = new Thickness(16),
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
                    new Button
                    {
                        Content = "OK",
                        HorizontalAlignment = HorizontalAlignment.Right,
                        MinWidth = 80
                    }
                }
            }
        };

        if (window.Content is StackPanel panel && panel.Children[1] is Button okButton)
            okButton.Click += (_, _) => window.Close();

        window.Show();
    }
}
