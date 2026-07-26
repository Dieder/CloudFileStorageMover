using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using CloudStorageMover.Core.Models;

namespace CloudStorageMover.App.Pages;

public partial class SlideshowPage : ContentPage, INotifyPropertyChanged
{
    private readonly List<PhotoItem> _photos;
    private System.Timers.Timer? _slideTimer;
    private int _currentIndex;
    private bool _isPlaying;

    public List<PhotoItem> Photos => _photos;

    public int CurrentIndex
    {
        get => _currentIndex;
        set { _currentIndex = value; OnPropertyChanged(); }
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        set { _isPlaying = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public SlideshowPage(List<PhotoItem> photos, string albumName)
    {
        InitializeComponent();
        _photos = photos;
        BindingContext = this;

        SlideshowCarousel.CurrentItemChanged += OnCurrentItemChanged;
    }

    private void OnCurrentItemChanged(object? sender, CurrentItemChangedEventArgs e)
    {
        if (e.CurrentItem is PhotoItem photo)
        {
            CurrentIndex = _photos.IndexOf(photo) + 1;
            TitleLabel.Text = photo.BlobName;
            CounterLabel.Text = $"{CurrentIndex} / {_photos.Count}";
        }
    }

    [RelayCommand]
    private void Close()
    {
        _slideTimer?.Stop();
        _slideTimer?.Dispose();
        Navigation.PopModalAsync();
    }

    [RelayCommand]
    private void Previous()
    {
        if (SlideshowCarousel.Position > 0)
            SlideshowCarousel.ScrollTo(SlideshowCarousel.Position - 1);
    }

    [RelayCommand]
    private void Next()
    {
        if (SlideshowCarousel.Position < _photos.Count - 1)
            SlideshowCarousel.ScrollTo(SlideshowCarousel.Position + 1);
    }

    [RelayCommand]
    private void TogglePlay()
    {
        IsPlaying = !IsPlaying;

        if (IsPlaying)
        {
            _slideTimer?.Dispose();
            _slideTimer = new System.Timers.Timer(3000);
            _slideTimer.Elapsed += (_, _) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (SlideshowCarousel.Position < _photos.Count - 1)
                        SlideshowCarousel.ScrollTo(SlideshowCarousel.Position + 1);
                    else
                    {
                        IsPlaying = false;
                        _slideTimer?.Stop();
                    }
                });
            };
            _slideTimer.AutoReset = true;
            _slideTimer.Start();
        }
        else
        {
            _slideTimer?.Stop();
        }
    }
}
