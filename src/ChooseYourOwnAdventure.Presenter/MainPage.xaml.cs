using ChooseYourOwnAdventure.Input;
using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
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

    private bool _hasStarted;

    public MainPage(
        IPresentationNavigator presentationNavigator,
        AdventureState adventureState,
        AdventureInputRouter inputRouter,
        OrbitInputBridge orbitInputBridge,
        AudienceVoteClient audienceVoteClient,
        LiveVoteState liveVoteState,
        LiveRaceState liveRaceState)
    {
        InitializeComponent();
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

        if (_presentationNavigator.Current == PresentationSlide.ChosenChampion)
        {
            _inputRouter.Trigger(position.Value.X < GameView.Width / 2d
                ? AdventureAction.Back : AdventureAction.Primary);
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
            _inputRouter.Trigger(AdventureAction.Primary);
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.GameLoop)
        {
            var isNextArea = position.Value.X >= GameView.Width * 0.75d
                && position.Value.Y >= GameView.Height * 0.88d;
            _inputRouter.Trigger(isNextArea ? AdventureAction.Right : AdventureAction.Primary);
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
                _ = _inputRouter.PulseAsync(AdventureAction.Primary, TimeSpan.FromMilliseconds(400));
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

        _inputRouter.Trigger(AdventureAction.Primary);
    }

    private async void OnAdventureAction(object? sender, AdventureAction action)
    {
        if (_presentationNavigator.IsTurning) return;

        switch (_presentationNavigator.Current)
        {
            case PresentationSlide.FirstDecision:
                if (action == AdventureAction.Secondary)
                    _presentationNavigator.GoTo(PresentationSlide.AfterParty);
                else if (action == AdventureAction.Back)
                    _presentationNavigator.Previous();
                else if (action == AdventureAction.Primary)
                    _presentationNavigator.Next();
                return;

            case PresentationSlide.ChooseChampion:
                await HandleChampionVoteAsync(action);
                return;

            case PresentationSlide.ChosenChampion:
                if (action is AdventureAction.Back or AdventureAction.Secondary)
                {
                    if (_adventureState.ChampionRenderingVisible)
                        _adventureState.HideChampionRendering();
                    else
                        _presentationNavigator.Previous();
                }
                else if (action == AdventureAction.Primary)
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

        if (action is AdventureAction.Back or AdventureAction.Secondary)
        {
            _presentationNavigator.Previous();
            return;
        }

        if (action != AdventureAction.Primary)
        {
            return;
        }

        _presentationNavigator.Next();
    }

    private async Task HandleChampionVoteAsync(AdventureAction action)
    {
        if (action == AdventureAction.Back)
        {
            _presentationNavigator.Previous();
            return;
        }
        if (action == AdventureAction.Secondary)
        {
            _adventureState.ResetLocalVote();
            if (_audienceVoteClient.IsConnected) await _audienceVoteClient.ResetVotingAsync();
            return;
        }

        if (action == AdventureAction.Left)
        {
            _adventureState.HighlightPreviousChampion();
            return;
        }

        if (action == AdventureAction.Right)
        {
            _adventureState.HighlightNextChampion();
            return;
        }

        if (action != AdventureAction.Primary)
        {
            return;
        }

        if (!_audienceVoteClient.IsConnected)
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
        if (action == AdventureAction.Primary)
        {
            _adventureState.AdvanceLoopStation();
            return;
        }

        if (action == AdventureAction.Left)
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
                _presentationNavigator.GoTo(PresentationSlide.VehicleControl);
            }
        }
    }

    private async Task HandleAudienceRaceAsync(AdventureAction action)
    {
        if (action is AdventureAction.Secondary or AdventureAction.Back)
        {
            await _audienceVoteClient.ResetRaceAsync();
            return;
        }

        if (action != AdventureAction.Primary)
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
