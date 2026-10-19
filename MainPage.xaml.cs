using Orbit.Engine;

namespace ChooseYourOwnAdventure;

public partial class MainPage : ContentPage
{
    private readonly IGameSceneManager _gameSceneManager;
    private readonly AdventureState _adventureState;
    private bool _hasStarted;

    public MainPage(
        IGameSceneManager gameSceneManager,
        AdventureState adventureState)
    {
        InitializeComponent();
        _gameSceneManager = gameSceneManager;
        _adventureState = adventureState;
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
    }

    private void OnGameViewTapped(object? sender, TappedEventArgs e)
    {
        var position = e.GetPosition(GameView);
        if (position is null)
        {
            return;
        }

        if (_adventureState.OpeningStage == OpeningStage.ChooseClass)
        {
            var third = GameView.Width / 3d;
            var chosen = position.Value.X switch
            {
                var x when x < third => PlayerClass.MauiWarrior,
                var x when x < third * 2d => PlayerClass.AsyncWizard,
                _ => PlayerClass.XamlRanger
            };

            _adventureState.ChooseClass(chosen);
            return;
        }

        _adventureState.Advance();
    }
}
