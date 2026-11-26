using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace ChooseYourOwnAdventure.PresenterNotes;

/// <summary>Owns the private notes window, never another presentation or Orbit scene.</summary>
public sealed class PresenterNotesWindow(IServiceProvider services)
{
    private Window? _notes;
    private Window? _presentation;
    private readonly Stopwatch _elapsed = new();
    public TimeSpan Elapsed => _elapsed.Elapsed;

    public void Attach(Window presentation)
    {
        _presentation = presentation;
        presentation.Destroying += (_, _) =>
        {
            if (_notes is { } notes) Application.Current?.CloseWindow(notes);
            _elapsed.Stop();
        };
    }

    public void StartSession()
    {
        if (!_elapsed.IsRunning) _elapsed.Start();
    }

    public void RestartTimer() => _elapsed.Restart();

    public Window CreateNotesWindow()
    {
        var page = services.GetRequiredService<PresenterNotesPage>();
        var window = new Window(page)
        {
            Title = "Presenter Notes — Choose Your Own Adventure",
            Width = 680, Height = 760, MinimumWidth = 420, MinimumHeight = 480
        };
#if MACCATALYST || __MACCATALYST__
        // Catalyst ignores initial Width/Height; a brief equal constraint sizes it.
        window.MinimumWidth = window.MaximumWidth = 680;
        window.MinimumHeight = window.MaximumHeight = 760;
        window.Created += (_, _) => window.Dispatcher.Dispatch(() =>
        {
            window.MaximumWidth = double.PositiveInfinity;
            window.MaximumHeight = double.PositiveInfinity;
            window.MinimumWidth = 420;
            window.MinimumHeight = 480;
        });
#endif
        _notes = window;
        window.Destroying += (_, _) =>
        {
            page.StopUpdating();
            if (ReferenceEquals(_notes, window)) _notes = null;
        };
        return window;
    }

    public void Show()
    {
#if MACCATALYST || __MACCATALYST__ || WINDOWS
        if (Application.Current is not { } app) return;
        if (_notes is { } existing) { app.ActivateWindow(existing); return; }
        app.OpenWindow(CreateNotesWindow());
#endif
    }

    public void FocusPresentation()
    {
#if MACCATALYST || __MACCATALYST__ || WINDOWS
        if (_presentation is { } presentation) Application.Current?.ActivateWindow(presentation);
#endif
    }
}
