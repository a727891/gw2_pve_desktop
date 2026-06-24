using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gw2PveDesktop.ViewModels;

public class ScheduleViewModel
{
    public FractalDayViewModel FractalsToday { get; set; } = new();
    public FractalDayViewModel FractalsTomorrow { get; set; } = new();
    public FractalDayViewModel FractalsCM { get; set; } = new();
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
