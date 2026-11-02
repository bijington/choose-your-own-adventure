using ChooseYourOwnAdventure.Input;
using Orbit.Engine;

namespace ChooseYourOwnAdventure;

public partial class MainPage : ContentPage
{
    private readonly IGameSceneManager _gameSceneManager;
    private readonly AdventureState _adventureState;
    private readonly AdventureInputRouter _inputRouter;
    private readonly OrbitInputBridge _orbitInputBridge;
    private bool _hasStarted;
    private bool _showingGameLoop;
    private bool _showingVehicleControl;

    public MainPage(
        IGameSceneManager gameSceneManager,
        AdventureState adventureState,
        AdventureInputRouter inputRouter,
        OrbitInputBridge orbitInputBridge)
    {
        InitializeComponent();
        _gameSceneManager = gameSceneManager;
        _adventureState = adventureState;
        _inputRouter = inputRouter;
        _orbitInputBridge = orbitInputBridge;

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
    }

    private void OnGameViewTapped(object? sender, TappedEventArgs e)
    {
        var position = e.GetPosition(GameView);
        if (position is null)
        {
            return;
        }

        if (_showingVehicleControl)
        {
            _ = _inputRouter.PulseAsync(AdventureAction.Primary, TimeSpan.FromMilliseconds(400));
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

    private void OnAdventureAction(object? sender, AdventureAction action)
    {
        if (_showingVehicleControl)
        {
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
            switch (action)
            {
                case AdventureAction.Left:
                    _adventureState.HighlightPreviousChampion();
                    return;
                case AdventureAction.Right:
                    _adventureState.HighlightNextChampion();
                    return;
                case AdventureAction.Primary:
                    _adventureState.ConfirmHighlightedChampion();
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
}
