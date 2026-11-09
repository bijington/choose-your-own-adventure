using Orbit.Engine;

namespace ChooseYourOwnAdventure.Presentation;

internal sealed class PresentationNavigator : IPresentationNavigator
{
    private readonly IGameSceneManager _gameSceneManager;
    private readonly AdventureState _adventureState;
    private readonly IReadOnlyList<PresentationSlideDefinition> _slides;
    private readonly IReadOnlyDictionary<PresentationSlide, int> _indices;

    private GameSceneView? _gameSceneView;
    private int _index;

    public PresentationNavigator(
        IGameSceneManager gameSceneManager,
        AdventureState adventureState,
        IReadOnlyList<PresentationSlideDefinition> slides)
    {
        _gameSceneManager = gameSceneManager;
        _adventureState = adventureState;
        _slides = slides;
        _indices = slides
            .Select((definition, index) => (definition.Slide, index))
            .ToDictionary(x => x.Slide, x => x.index);
    }

    public PresentationSlide Current => _slides[_index].Slide;

    public void Start(GameSceneView gameSceneView)
    {
        _gameSceneView = gameSceneView;
        _index = 0;
        LoadCurrent();
    }

    public bool Next()
    {
        if (_index >= _slides.Count - 1)
        {
            return false;
        }

        _index++;
        LoadCurrent();
        return true;
    }

    public bool Previous()
    {
        if (_index <= 0)
        {
            return false;
        }

        _index--;
        LoadCurrent();
        return true;
    }

    public bool GoTo(PresentationSlide slide)
    {
        if (!_indices.TryGetValue(slide, out var index))
        {
            return false;
        }

        _index = index;
        LoadCurrent();
        return true;
    }

    private void LoadCurrent()
    {
        if (_gameSceneView is null)
        {
            throw new InvalidOperationException(
                "The presentation has not been started with a GameSceneView.");
        }

        var definition = _slides[_index];
        definition.OnEnter?.Invoke(_adventureState);

        _gameSceneManager.LoadScene(definition.SceneType, _gameSceneView);
        _gameSceneManager.Start();
    }
}
