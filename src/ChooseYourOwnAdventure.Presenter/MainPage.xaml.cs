using ChooseYourOwnAdventure.Input;
using ChooseYourOwnAdventure.Live;
using Orbit.Engine;

namespace ChooseYourOwnAdventure;

public partial class MainPage : ContentPage
{
    private readonly IGameSceneManager _gameSceneManager;
    private readonly AdventureState _adventureState;
    private readonly AdventureInputRouter _inputRouter;
    private readonly OrbitInputBridge _orbitInputBridge;
    private readonly AudienceVoteClient _audienceVoteClient;
    private readonly LiveVoteState _liveVoteState;
    private readonly LiveRaceState _liveRaceState;
    private bool _hasStarted;
    private bool _showingGameLoop;
    private bool _showingVehicleControl;
    private bool _showingAudienceRace;
    private static readonly TimeSpan VoteLockInDuration = TimeSpan.FromSeconds(3);

    public MainPage(
        IGameSceneManager gameSceneManager,
        AdventureState adventureState,
        AdventureInputRouter inputRouter,
        OrbitInputBridge orbitInputBridge,
        AudienceVoteClient audienceVoteClient,
        LiveVoteState liveVoteState,
        LiveRaceState liveRaceState)
    {
        InitializeComponent();
        _gameSceneManager = gameSceneManager;
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

        _gameSceneManager.LoadScene<Scenes.OpeningScene>(GameView);
        _gameSceneManager.Start();
        _hasStarted = true;

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

        if (_showingAudienceRace)
        {
            _inputRouter.Trigger(AdventureAction.Primary);
            return;
        }

        if (_showingVehicleControl)
        {
            var isNextArea = position.Value.X >= GameView.Width * 0.75d
                && position.Value.Y >= GameView.Height * 0.86d;

            if (isNextArea)
            {
                ShowAudienceRace();
            }
            else
            {
                _ = _inputRouter.PulseAsync(AdventureAction.Primary, TimeSpan.FromMilliseconds(400));
            }

            return;
        }

        if (_showingGameLoop)
        {
            // Keep touch equivalent to the keyboard/controller flow. The bottom-right
            // NEXT control advances the presentation; tapping elsewhere advances the
            // champion around the conceptual loop.
            var isNextArea = position.Value.X >= GameView.Width * 0.75d
                && position.Value.Y >= GameView.Height * 0.88d;

            _inputRouter.Trigger(isNextArea
                ? AdventureAction.Right
                : AdventureAction.Primary);
            return;
        }

        if (_adventureState.OpeningStage == OpeningStage.ChooseChampion)
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

        _inputRouter.Trigger(AdventureAction.Primary);
    }

    private async void OnAdventureAction(object? sender, AdventureAction action)
    {
        if (_showingAudienceRace)
        {
            if (action == AdventureAction.Primary)
            {
                var race = _liveRaceState.Snapshot;
                if (race.Phase is RacePhase.Ready or RacePhase.Finished)
                {
                    await _audienceVoteClient.StartRaceAsync();
                }
                return;
            }

            if (action is AdventureAction.Secondary or AdventureAction.Back)
            {
                await _audienceVoteClient.ResetRaceAsync();
            }

            return;
        }

        if (_showingVehicleControl)
        {
            // Up/W is deliberately reserved as the presenter navigation command on this demo scene.
            // Left/right/primary/secondary remain available to drive the themed vehicle.
            if (action == AdventureAction.Up)
            {
                ShowAudienceRace();
            }
            return;
        }

        if (_showingGameLoop)
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
                    return;
                }

                _gameSceneManager.LoadScene<Scenes.VehicleControlScene>(GameView);
                _gameSceneManager.Start();
                _showingGameLoop = false;
                _showingVehicleControl = true;
            }

            return;
        }

        if (_adventureState.OpeningStage == OpeningStage.ChooseChampion)
        {
            if ((action == AdventureAction.Back || action == AdventureAction.Secondary) && _audienceVoteClient.IsConnected)
            {
                await _audienceVoteClient.ResetVotingAsync();
                return;
            }

            switch (action)
            {
                case AdventureAction.Left:
                    _adventureState.HighlightPreviousChampion();
                    return;
                case AdventureAction.Right:
                    _adventureState.HighlightNextChampion();
                    return;
                case AdventureAction.Primary:
                    if (!_audienceVoteClient.IsConnected)
                    {
                        _adventureState.ConfirmHighlightedChampion();
                        return;
                    }

                    var vote = _liveVoteState.Snapshot;
                    switch (vote.Phase)
                    {
                        case VotePhase.Ready:
                            await _audienceVoteClient.OpenVotingAsync();
                            return;
                        case VotePhase.Open:
                            await _audienceVoteClient.CloseVotingAsync();
                            return;
                        case VotePhase.Closed:
                            // Give the presenter screen time to run the lock-in countdown
                            // before allowing the winner reveal. Repeated button presses during
                            // the countdown are intentionally ignored.
                            if (_liveVoteState.PhaseElapsed < VoteLockInDuration)
                            {
                                return;
                            }

                            await _audienceVoteClient.RevealWinnerAsync();
                            return;
                        case VotePhase.Revealed:
                            if (vote.RevealedChampion is { } winner)
                            {
                                _adventureState.ChooseChampion(winner);
                            }
                            else
                            {
                                // A tie/no-vote reveal is a useful live moment: reopen voting
                                // rather than secretly applying a tie-break rule.
                                await _audienceVoteClient.OpenVotingAsync();
                            }
                            return;
                    }

                    return;
                default:
                    return;
            }
        }

        if (action != AdventureAction.Primary)
        {
            return;
        }

        if (_adventureState.OpeningStage == OpeningStage.Complete)
        {
            _gameSceneManager.LoadScene<Scenes.GameLoopScene>(GameView);
            _gameSceneManager.Start();
            _showingGameLoop = true;
            return;
        }

        _adventureState.AdvanceOpening();
    }
    private void ShowAudienceRace()
    {
        _gameSceneManager.LoadScene<Scenes.AudienceRaceScene>(GameView);
        _gameSceneManager.Start();
        _showingVehicleControl = false;
        _showingGameLoop = false;
        _showingAudienceRace = true;
    }

}
