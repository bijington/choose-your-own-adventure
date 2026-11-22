namespace ChooseYourOwnAdventure.Controls;

public partial class MauiAnimationShowcase : ContentView
{
    public event EventHandler? ContinueRequested;

    private void OnContinueClicked(object? sender, EventArgs e) => ContinueRequested?.Invoke(this, EventArgs.Empty);

    private bool _isLoaded;
    private bool _isAnimating;

    private const string AnimationName = "MauiShowcase";

    public MauiAnimationShowcase()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            _isLoaded = true;
            IsVisibleChanged();
        };
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(IsVisible)) IsVisibleChanged();
        };
        Unloaded += (_, _) =>
        {
            _isLoaded = false;
            Stop();
        };
    }

    private void IsVisibleChanged()
    {
        if (IsVisible) Play();
        else Stop();
    }

    private void OnReplayClicked(object? sender, EventArgs e) => Play();

    private void Stop()
    {
        // Visibility can change during XAML construction or after handler teardown.
        if (_isAnimating && Handler?.MauiContext is not null)
            this.AbortAnimation(AnimationName);
        _isAnimating = false;
    }

    private void Play()
    {
        if (!_isLoaded || !IsVisible || Handler?.MauiContext is null) return;
        Stop();
        _isAnimating = true;
        // These are real MAUI views and MAUI animations, independent of Orbit's renderer.
        new Animation(t =>
        {
            var wave = Math.Sin(t * Math.PI * 2);
            Bouncer.TranslationY = -24 * Math.Pow(Math.Sin(t * Math.PI), 2);
            Spinner.Rotation = 360 * Easing.CubicInOut.Ease(t);
            Spinner.Scale = 1 + .16 * wave;
            Pulse.Scale = 1 + .13 * Math.Pow(Math.Sin(t * Math.PI * 2), 4);
            Pulse.Opacity = .65 + .35 * Math.Pow(Math.Sin(t * Math.PI * 2), 2);
        }).Commit(this, AnimationName, length: 2400, repeat: () => _isLoaded && IsVisible);
    }
}
