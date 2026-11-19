using ChooseYourOwnAdventure.Input;
using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
using ChooseYourOwnAdventure.PresenterNotes;
using Orbit.Engine;
using ChooseYourOwnAdventure.Scenes;

namespace ChooseYourOwnAdventure;

public partial class MainPage : ContentPage
{
    private static readonly TimeSpan VoteRevealDuration = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan VoteLockInDuration = TimeSpan.FromSeconds(3);

    private readonly IPresentationNavigator _presentationNavigator;
    private readonly AdventureState _adventureState;
    private readonly AdventureInputRouter _inputRouter;
    private readonly OrbitInputBridge _orbitInputBridge;
    private readonly AudienceVoteClient _audienceVoteClient;
    private readonly LiveVoteState _liveVoteState;
    private readonly LiveRaceState _liveRaceState;

    private readonly PresenterNotesWindow _notes;
    private bool _hasStarted;

    public MainPage(
        IPresentationNavigator presentationNavigator,
        AdventureState adventureState,
        AdventureInputRouter inputRouter,
        OrbitInputBridge orbitInputBridge,
        AudienceVoteClient audienceVoteClient,
        LiveVoteState liveVoteState,
        LiveRaceState liveRaceState,
        PresenterNotesWindow notes,
        IReadOnlyList<PresentationSlideDefinition> slides)
    {
        InitializeComponent();
        _notes = notes;
#if MACCATALYST || __MACCATALYST__ || WINDOWS
        var presenterMenu = new MenuBarItem { Text = "Presenter" };
        presenterMenu.Add(new MenuFlyoutItem { Text = "Show Presenter Notes", Command = new Command(_notes.Show) });
        presenterMenu.Add(PresenterMenuItems.CreateSlideNavigation(slides, slide =>
        {
            if (_hasStarted) presentationNavigator.GoTo(slide);
        }));
        MenuBarItems.Add(presenterMenu);
#endif
        _presentationNavigator = presentationNavigator;
        _adventureState = adventureState;
        _inputRouter = inputRouter;
        _orbitInputBridge = orbitInputBridge;
        _audienceVoteClient = audienceVoteClient;
        _liveVoteState = liveVoteState;
        _liveRaceState = liveRaceState;

        _inputRouter.ActionTriggered += OnAdventureAction;
    }

    private void OnGameViewLoaded(object? sender, EventArgs e)
    {
        if (_hasStarted)
        {
            return;
        }

        _presentationNavigator.Start(GameView);
        _hasStarted = true;
        _notes.StartSession();
#if MACCATALYST || __MACCATALYST__ || WINDOWS
        // The audience scene is already running; opening notes cannot block startup.
        Dispatcher.Dispatch(_notes.Show);
#endif

        // Input/network discovery must never block the presentation rendering.
        _ = _orbitInputBridge.StartAsync();
        _ = _audienceVoteClient.StartAsync();
    }

    private void OnGameViewTapped(object? sender, TappedEventArgs e)
    {
        if (_presentationNavigator.IsTurning) return;

        var position = e.GetPosition(GameView);
        if (position is null)
        {
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.FirstDecision)
        {
            if (position.Value.X < GameView.Width / 2d)
                _presentationNavigator.Previous();
            else if (FirstDecisionScene.IsAfterPartyChoice(
                new Microsoft.Maui.Graphics.RectF(0, 0, (float)GameView.Width, (float)GameView.Height),
                new Microsoft.Maui.Graphics.PointF((float)position.Value.X, (float)position.Value.Y)))
                _presentationNavigator.GoTo(PresentationSlide.AfterParty);
            else
                _presentationNavigator.Next();
            return;
        }

        if (_presentationNavigator.Current is PresentationSlide.ChosenChampion
            or PresentationSlide.SignalRArchitecture or PresentationSlide.OrbitOverview)
        {
            _inputRouter.Trigger(position.Value.X < GameView.Width / 2d
                ? AdventureAction.Previous : AdventureAction.Confirm);
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.ChooseChampion
            && position.Value.X < GameView.Width / 2d)
        {
            _presentationNavigator.Previous();
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.ChooseChampion && _adventureState.OpeningStage == OpeningStage.ChooseChampion)
        {
            var chosen = ChampionSelectionLayout.HitTest(
                new Microsoft.Maui.Graphics.RectF(0, 0, (float)GameView.Width, (float)GameView.Height),
                new Microsoft.Maui.Graphics.PointF((float)position.Value.X, (float)position.Value.Y));
            if (chosen is { } champion) _adventureState.HighlightChampion(champion);
            _inputRouter.Trigger(AdventureAction.Confirm);
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.GameLoop)
        {
            var isNextArea = position.Value.X >= GameView.Width * 0.75d
                && position.Value.Y >= GameView.Height * 0.88d;
            _inputRouter.Trigger(position.Value.X < GameView.Width / 2d
                ? AdventureAction.Cancel : isNextArea ? AdventureAction.Right : AdventureAction.Confirm);
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.VehicleControl)
        {
            var isNextArea = position.Value.X >= GameView.Width * 0.72d
                && position.Value.Y >= GameView.Height * 0.84d;

            if (isNextArea)
            {
                _presentationNavigator.GoTo(PresentationSlide.SignalRChapter);
            }
            else
            {
                _ = _inputRouter.PulseAsync(AdventureAction.Confirm, TimeSpan.FromMilliseconds(400));
            }
            return;
        }

        // Each facing page is a navigation target on ordinary spreads.
        // The cover and interactive spreads retain their own touch controls.
        if (_presentationNavigator.Current is not (PresentationSlide.Title
                or PresentationSlide.ChooseChampion or PresentationSlide.AudienceRace)
            && position.Value.X < GameView.Width / 2d)
        {
            _presentationNavigator.Previous();
            return;
        }

        _inputRouter.Trigger(AdventureAction.Confirm);
    }

    private async void OnAdventureAction(object? sender, AdventureAction action)
    {
        if (_presentationNavigator.IsTurning) return;

        // Navigation never starts or resets a vote/race. Confirmation owns those commands.
        if (action == AdventureAction.Previous)
        {
            if (_presentationNavigator.Current == PresentationSlide.GameLoop
                && _adventureState.GameLoopStage == GameLoopStage.FirmwareComparison)
            {
                _adventureState.ShowGameLoopOverview();
                return;
            }
            if (_presentationNavigator.Current == PresentationSlide.OrbitOverview
                && _adventureState.PreviousOrbitOverviewStage()) return;
            if (_presentationNavigator.Current == PresentationSlide.SignalRArchitecture
                && _adventureState.PreviousSignalRStage()) return;
            if (_presentationNavigator.Current == PresentationSlide.ChosenChampion
                && _adventureState.ChampionRenderingVisible)
                _adventureState.HideChampionRendering();
            else
                _presentationNavigator.Previous();
            return;
        }
        if (action == AdventureAction.Next)
        {
            var resolved = PresentationNavigation.ResolveNext(_presentationNavigator.Current,
                _adventureState, _liveVoteState.Snapshot, _liveRaceState.Snapshot, _audienceVoteClient.IsConnected);
            if (resolved is null) return;
            if (resolved == AdventureAction.Next)
            {
                _presentationNavigator.Next();
                return;
            }
            action = resolved.Value;
        }

        switch (_presentationNavigator.Current)
        {
            case PresentationSlide.FirstDecision:
                if (action == AdventureAction.Cancel)
                    _presentationNavigator.GoTo(PresentationSlide.AfterParty);
                else if (action == AdventureAction.Confirm)
                    _presentationNavigator.Next();
                return;

            case PresentationSlide.ChooseChampion:
                await HandleChampionVoteAsync(action);
                return;

            case PresentationSlide.ChosenChampion:
                if (action == AdventureAction.Cancel)
                {
                    if (_adventureState.ChampionRenderingVisible)
                        _adventureState.HideChampionRendering();
                    else
                        _presentationNavigator.Previous();
                }
                else if (action == AdventureAction.Confirm)
                {
                    if (!_adventureState.ChampionRenderingVisible)
                        _adventureState.ShowChampionRendering();
                    else
                        _presentationNavigator.Next();
                }
                return;

            case PresentationSlide.GameLoop:
                HandleGameLoop(action);
                return;

            case PresentationSlide.OrbitOverview:
                if (action == AdventureAction.Confirm)
                {
                    if (!_adventureState.AdvanceOrbitOverviewStage()) _presentationNavigator.Next();
                }
                else if (action == AdventureAction.Cancel)
                {
                    if (!_adventureState.PreviousOrbitOverviewStage()) _presentationNavigator.Previous();
                }
                return;

            case PresentationSlide.SignalRArchitecture:
                if (action == AdventureAction.Confirm)
                {
                    if (!_adventureState.AdvanceSignalRStage()) _presentationNavigator.Next();
                }
                else if (action == AdventureAction.Cancel)
                {
                    if (!_adventureState.PreviousSignalRStage()) _presentationNavigator.Previous();
                }
                return;

            case PresentationSlide.VehicleControl:
                if (action == AdventureAction.Up)
                {
                    _presentationNavigator.GoTo(PresentationSlide.SignalRChapter);
                }
                return;

            case PresentationSlide.AudienceRace:
                await HandleAudienceRaceAsync(action);
                return;
        }

        if (action == AdventureAction.Cancel)
        {
            _presentationNavigator.Previous();
            return;
        }

        if (action != AdventureAction.Confirm)
        {
            return;
        }

        _presentationNavigator.Next();
    }

    private async Task HandleChampionVoteAsync(AdventureAction action)
    {
        if (action == AdventureAction.Cancel)
        {
            _adventureState.ResetLocalVote();
            if (_audienceVoteClient.IsConnected) await _audienceVoteClient.ResetVotingAsync();
            return;
        }

        if (action is AdventureAction.Left or AdventureAction.Up)
        {
            _adventureState.HighlightPreviousChampion();
            return;
        }

        if (action is AdventureAction.Right or AdventureAction.Down)
        {
            _adventureState.HighlightNextChampion();
            return;
        }

        if (action != AdventureAction.Confirm)
        {
            return;
        }

        if (_adventureState.LocalVoteWinner is not null || !_audienceVoteClient.IsConnected)
        {
            if (_adventureState.LocalVoteWinner is not { } localWinner)
            {
                _adventureState.RevealLocalVote();
            }
            else if (DateTimeOffset.UtcNow - _adventureState.LocalVoteRevealedAt >= VoteRevealDuration)
            {
                _adventureState.ChooseChampion(localWinner);
                _presentationNavigator.GoTo(PresentationSlide.ChosenChampion);
            }
            return;
        }

        var vote = _liveVoteState.Snapshot;
        switch (vote.Phase)
        {
            case VotePhase.Ready:
                await _audienceVoteClient.OpenVotingAsync();
                break;
            case VotePhase.Open:
                await _audienceVoteClient.CloseVotingAsync();
                break;
            case VotePhase.Closed:
                if (_liveVoteState.PhaseElapsed >= VoteLockInDuration)
                {
                    await _audienceVoteClient.RevealWinnerAsync();
                }
                break;
            case VotePhase.Revealed when vote.RevealedChampion is { } winner:
                if (_liveVoteState.PhaseElapsed < VoteRevealDuration) break;
                _adventureState.ChooseChampion(winner);
                _presentationNavigator.GoTo(PresentationSlide.ChosenChampion);
                break;
            case VotePhase.Revealed:
                await _audienceVoteClient.OpenVotingAsync();
                break;
        }
    }

    private void HandleGameLoop(AdventureAction action)
    {
        if (_adventureState.GameLoopStage == GameLoopStage.FirmwareComparison)
        {
            if (action is AdventureAction.Confirm or AdventureAction.Right)
                _presentationNavigator.Next();
            else if (action is AdventureAction.Cancel or AdventureAction.Left)
                _adventureState.ShowGameLoopOverview();
            return;
        }
        if (action == AdventureAction.Confirm)
        {
            _adventureState.AdvanceLoopStation();
            return;
        }

        if (action is AdventureAction.Left or AdventureAction.Cancel)
        {
            _adventureState.PreviousLoopStation();
            return;
        }

        if (action == AdventureAction.Right)
        {
            if (_adventureState.GameLoopStage == GameLoopStage.Overview)
            {
                _adventureState.ShowFirmwareComparison();
            }
            else
            {
                _presentationNavigator.Next();
            }
        }
    }

    private async Task HandleAudienceRaceAsync(AdventureAction action)
    {
        if (action == AdventureAction.Cancel)
        {
            await _audienceVoteClient.ResetRaceAsync();
            return;
        }

        if (action != AdventureAction.Confirm)
        {
            return;
        }

        var race = _liveRaceState.Snapshot;
        if (race.Phase == RacePhase.Ready)
        {
            await _audienceVoteClient.StartRaceAsync();
        }
        else if (race.Phase == RacePhase.Finished)
        {
            _presentationNavigator.GoTo(PresentationSlide.EpilogueChapter);
        }
    }

}
