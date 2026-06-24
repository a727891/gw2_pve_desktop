using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Gw2PveDesktop.Services;
using Gw2PveDesktop.ViewModels;

namespace Gw2PveDesktop.Avalonia;

public partial class PopupWindow : Window
{
    private readonly ScheduleService _scheduleService;
    private readonly BountyIconCacheService _iconCache;
    private readonly DispatcherTimer _countdownTimer;
    private bool _wasBeforeReset = true;
    private bool _showFractalCMs;

    private const int BackgroundAssetId = 1909321;

    public PopupWindow(ScheduleService scheduleService, BountyIconCacheService iconCache)
    {
        _scheduleService = scheduleService;
        _iconCache = iconCache;
        InitializeComponent();
        Closing += PopupWindow_Closing;
        Opened += PopupWindow_Opened;

        try
        {
            var iconStream = AssetLoader.Open(new Uri("avares://Gw2PveDesktop/Assets/1128644.png"));
            Icon = new WindowIcon(iconStream);
        }
        catch
        {
            // Optional window icon.
        }

        _countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _countdownTimer.Tick += (_, _) => UpdateCountdown();
        _countdownTimer.Start();
        UpdateCountdown();

        Loaded += PopupWindow_Loaded;
    }

    private void PopupWindow_Opened(object? sender, EventArgs e)
    {
        PositionNearTray();
    }

    private void PositionNearTray()
    {
        var screen = Screens.Primary ?? Screens.All.FirstOrDefault();
        if (screen == null) return;

        var workingArea = screen.WorkingArea;
        Position = new PixelPoint(
            workingArea.Right - (int)Bounds.Width - 10,
            workingArea.Bottom - (int)Bounds.Height - 10);
    }

    private async void PopupWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        var path = await _iconCache.GetImagePathAsync(BackgroundAssetId).ConfigureAwait(false);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (path != null)
            {
                try
                {
                    var image = new Bitmap(path);
                    RootBorder.Background = new ImageBrush(image)
                    {
                        Stretch = Stretch.UniformToFill,
                        AlignmentX = AlignmentX.Center,
                        AlignmentY = AlignmentY.Center
                    };
                    return;
                }
                catch
                {
                    // Fall through to solid background.
                }
            }

            RootBorder.Background = new SolidColorBrush(Color.FromRgb(0x2a, 0x2a, 0x2e));
        });
    }

    private void PopupWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private void UpdateCountdown()
    {
        var timeUntilReset = ResetService.TimeUntilReset;
        if (timeUntilReset.TotalSeconds <= 0)
        {
            if (_wasBeforeReset)
            {
                _wasBeforeReset = false;
                RefreshData();
            }
            ResetCountdownText.Text = "Reset in 0m 0s";
        }
        else
        {
            _wasBeforeReset = true;
            ResetCountdownText.Text = $"Reset in {ResetService.GetCountdownString()}";
        }
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void FractalsDailiesButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_showFractalCMs)
        {
            _showFractalCMs = false;
            ApplyFractalsMode(_scheduleService.GetSchedule());
        }
    }

    private void FractalsCMButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!_showFractalCMs)
        {
            _showFractalCMs = true;
            ApplyFractalsMode(_scheduleService.GetSchedule());
        }
    }

    private void ApplyFractalsMode(ScheduleViewModel schedule)
    {
        var titleBrush = (IBrush)Resources["GuildWarsBodyBrush"]!;
        var mutedBrush = (IBrush)Resources["GuildWarsMutedBrush"]!;

        if (_showFractalCMs)
        {
            FractalsDailiesPanel.IsVisible = false;
            FractalsCMPanel.IsVisible = true;
            FractalsDailiesButton.Foreground = mutedBrush;
            FractalsCMButton.Foreground = titleBrush;

            var cm = schedule.FractalsCM.Fractals;
            var half = (cm.Count + 1) / 2;
            FractalsCMLeftList.ItemsSource = cm.Take(half).ToList();
            FractalsCMRightList.ItemsSource = cm.Skip(half).ToList();
            _ = LoadInstabilityIconsAsync(cm, Enumerable.Empty<FractalEntryViewModel>());
        }
        else
        {
            FractalsDailiesPanel.IsVisible = true;
            FractalsCMPanel.IsVisible = false;
            FractalsDailiesButton.Foreground = titleBrush;
            FractalsCMButton.Foreground = mutedBrush;

            FractalsTodayList.ItemsSource = schedule.FractalsToday.Fractals;
            FractalsTomorrowList.ItemsSource = schedule.FractalsTomorrow.Fractals;
            _ = LoadInstabilityIconsAsync(schedule.FractalsToday.Fractals, schedule.FractalsTomorrow.Fractals);
        }
    }

    public void RefreshData()
    {
        var schedule = _scheduleService.GetSchedule();
        BountiesTodayList.ItemsSource = schedule.BountiesToday.Bounties;
        BountiesTomorrowList.ItemsSource = schedule.BountiesTomorrow.Bounties;
        _ = LoadBountyIconsAsync(schedule.BountiesToday.Bounties, schedule.BountiesTomorrow.Bounties);
        ApplyFractalsMode(schedule);
    }

    private async Task LoadInstabilityIconsAsync(IEnumerable<FractalEntryViewModel> today, IEnumerable<FractalEntryViewModel> tomorrow)
    {
        var allInstabilities = today.Concat(tomorrow).SelectMany(f => f.Instabilities);
        foreach (var entry in allInstabilities)
        {
            if (entry.AssetId is not { } assetId) continue;
            var path = await _iconCache.GetImagePathAsync(assetId).ConfigureAwait(false);
            if (path != null)
                await Dispatcher.UIThread.InvokeAsync(() => entry.ImagePath = path);
        }
    }

    private async Task LoadBountyIconsAsync(IEnumerable<BountyEntryViewModel> today, IEnumerable<BountyEntryViewModel> tomorrow)
    {
        var all = today.Concat(tomorrow);
        foreach (var entry in all)
        {
            if (entry.AssetId is not { } assetId) continue;
            var path = await _iconCache.GetImagePathAsync(assetId).ConfigureAwait(false);
            if (path != null)
                await Dispatcher.UIThread.InvokeAsync(() => entry.ImagePath = path);
        }
    }
}
