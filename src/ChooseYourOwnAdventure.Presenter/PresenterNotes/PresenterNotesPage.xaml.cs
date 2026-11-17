using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Dispatching;

namespace ChooseYourOwnAdventure.PresenterNotes;

public sealed partial class PresenterNotesPage : ContentPage
{
    private readonly IPresentationNavigator _navigator;
    private readonly AdventureState _state;
    private readonly LiveVoteState _vote;
    private readonly LiveRaceState _race;
    private readonly AudienceVoteClient _audience;
    private readonly PresenterNotesWindow _windows;
    private readonly PresenterNotesGuide _guide;
    private readonly IReadOnlyList<PresentationSlideDefinition> _slides;
    private readonly IDispatcherTimer _timer;
    private string? _lastContent;

    public PresenterNotesPage(IPresentationNavigator navigator, AdventureState state,
        LiveVoteState vote, LiveRaceState race, AudienceVoteClient audience,
        PresenterNotesWindow windows, PresenterNotesGuide guide,
        IReadOnlyList<PresentationSlideDefinition> slides)
    {
        _navigator = navigator; _state = state; _vote = vote; _race = race;
        _audience = audience; _windows = windows; _guide = guide; _slides = slides;
        InitializeComponent();
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(250);
        _timer.Tick += (_, _) => RefreshNotes();
        Loaded += (_, _) => { RefreshNotes(); _timer.Start(); };
        Unloaded += (_, _) => _timer.Stop();
    }

    public void StopUpdating() => _timer.Stop();

    private void RefreshNotes()
    {
        _clock.Text = _windows.Elapsed.ToString(@"hh\:mm\:ss");
        var slide = _navigator.Current;
        var current = _slides.First(definition => definition.Slide == slide);
        var cue = PresenterCueFactory.Create(slide, _state, _vote.Snapshot, _race.Snapshot, _audience.IsConnected);
        var index = _slides.ToList().FindIndex(definition => definition.Slide == slide);
        var next = current.Next ?? (index + 1 < _slides.Count ? _slides[index + 1].Slide : (PresentationSlide?)null);
        var title = PresenterCueFactory.TitleFor(slide);
        var section = PresentationChapter.LabelFor(slide) ?? "CHOOSE YOUR OWN ADVENTURE";
        section += slide == PresentationSlide.Title ? " · COVER"
            : " · PAGE " + PresentationTheme.GetPageNumber(slide, _state.Champion);
        var points = string.IsNullOrEmpty(cue.TalkingPoints) ? _guide.ForScene(current.SceneType.Name) : cue.TalkingPoints;
        var upcoming = next is { } nextSlide
            ? "Next spread: " + PresenterCueFactory.TitleFor(nextSlide)
            : "Final spread — thank the audience.";
        var status = $"Audience: {(_audience.IsConnected ? "connected" : "offline")}   ·   Votes: {_vote.Snapshot.Total}   ·   Race: {_race.Snapshot.Phase}";
        var content = $"{title}|{section}|{cue}|{points}|{upcoming}|{status}|{_navigator.IsTurning}";
        if (_lastContent == content) return;
        _lastContent = content;
        _title.Text = title;
        _section.Text = section;
        _beat.Text = _navigator.IsTurning ? "Turning the page…" : cue.Beat;
        _notes.Text = points;
        _controls.Text = cue.Controls;
        _nextAction.Text = cue.NextAction;
        _nextSpread.Text = upcoming;
        _status.Text = status;
    }

    private void OnReturnToPresentation(object? sender, EventArgs e) => _windows.FocusPresentation();
}
