using ChooseYourOwnAdventure.Input;
using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
using Orbit.Engine;

namespace ChooseYourOwnAdventure;

public partial class MainPage : ContentPage
{
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
        var position = e.GetPosition(GameView);
        if (position is null)
        {
            return;
        }

        if (_presentationNavigator.Current == PresentationSlide.ChooseChampion && _adventureState.OpeningStage == OpeningStage.ChooseChampion)
        {
            var third = GameView.Width / 3d;
            var chosen = position.Value.X switch
            {
                var x when x < third => Champion.RacingDriver,
                var x when x < third * 2d => Champion.Charioteer,
                _ => Champion.SpacePilot
            };

            _adventureState.HighlightChampion(chosen);
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
                _presentationNavigator.GoTo(PresentationSlide.SignalRTransition);
            }
            else
            {
                _ = _inputRouter.PulseAsync(AdventureAction.Primary, TimeSpan.FromMilliseconds(400));
            }
            return;
        }

        _inputRouter.Trigger(AdventureAction.Primary);
    }

    private async void OnAdventureAction(object? sender, AdventureAction action)
    {
        switch (_presentationNavigator.Current)
        {
            case PresentationSlide.ChooseChampion:
                await HandleChampionVoteAsync(action);
                return;

            case PresentationSlide.GameLoop:
                HandleGameLoop(action);
                return;

            case PresentationSlide.VehicleControl:
                if (action == AdventureAction.Up)
                {
                    _presentationNavigator.GoTo(PresentationSlide.SignalRTransition);
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
        if ((action is AdventureAction.Back or AdventureAction.Secondary) && _audienceVoteClient.IsConnected)
        {
            await _audienceVoteClient.ResetVotingAsync();
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
            _adventureState.ConfirmHighlightedChampion();
            _presentationNavigator.GoTo(PresentationSlide.AddGameObject);
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
                _adventureState.ChooseChampion(winner);
                _presentationNavigator.GoTo(PresentationSlide.AddGameObject);
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
            _presentationNavigator.GoTo(PresentationSlide.Recap);
        }
    }

}
