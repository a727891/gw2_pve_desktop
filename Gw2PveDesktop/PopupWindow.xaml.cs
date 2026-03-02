using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Gw2PveDesktop.Services;

namespace Gw2PveDesktop;

public partial class PopupWindow : Window
{
    private readonly ScheduleService _scheduleService;
    private readonly DispatcherTimer _countdownTimer;
    private bool _wasBeforeReset = true;

    public PopupWindow(ScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
        InitializeComponent();
        StateChanged += PopupWindow_StateChanged;
        Closing += PopupWindow_Closing;

        var iconSource = LoadWindowIconFromEmbeddedPng();
        if (iconSource != null)
            Icon = iconSource;

        _countdownTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _countdownTimer.Tick += (_, _) => UpdateCountdown();
        _countdownTimer.Start();
        UpdateCountdown();
    }

    private void PopupWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private void PopupWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
            Hide();
        }
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

    /// <summary>Load window icon from embedded PNG (preserves transparency).</summary>
    private static ImageSource? LoadWindowIconFromEmbeddedPng()
    {
        var asm = Assembly.GetExecutingAssembly();
        var name = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(".1128644.png", StringComparison.OrdinalIgnoreCase));
        if (name == null) return null;
        using var stream = asm.GetManifestResourceStream(name);
        return stream != null ? BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad) : null;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    public void RefreshData()
    {
        var schedule = _scheduleService.GetSchedule();
        FractalsTodayList.ItemsSource = schedule.FractalsToday.Fractals;
        FractalsTomorrowList.ItemsSource = schedule.FractalsTomorrow.Fractals;
        BountiesTodayList.ItemsSource = schedule.BountiesToday.Bounties;
        BountiesTomorrowList.ItemsSource = schedule.BountiesTomorrow.Bounties;
    }
}
