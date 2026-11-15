using Orbit.Engine;
using Microsoft.Extensions.DependencyInjection;

namespace ChooseYourOwnAdventure.Presentation;

internal sealed class PresentationNavigator : IPresentationNavigator, IDisposable
{
    private readonly IGameSceneManager _gameSceneManager;
    private readonly AdventureState _adventureState;
    private readonly IReadOnlyList<PresentationSlideDefinition> _slides;
    private readonly IReadOnlyDictionary<PresentationSlide, int> _indices;

    private readonly BookScene _book;
    private readonly IServiceScopeFactory _scopeFactory;
    private IServiceScope? _contentScope;
    private GameSceneView? _gameSceneView;
    private int _index;

    public PresentationNavigator(
        IGameSceneManager gameSceneManager,
        AdventureState adventureState,
        IReadOnlyList<PresentationSlideDefinition> slides,
        BookScene book,
        IServiceScopeFactory scopeFactory)
    {
        _book = book;
        _scopeFactory = scopeFactory;
        _gameSceneManager = gameSceneManager;
        _adventureState = adventureState;
        _slides = slides;
        _indices = slides
            .Select((definition, index) => (definition.Slide, index))
            .ToDictionary(x => x.Slide, x => x.index);
    }

    public bool IsTurning => _book.IsTurning;

    public PresentationSlide Current => _slides[_index].Slide;

    public void Start(GameSceneView gameSceneView)
    {
        _gameSceneView = gameSceneView;
        _index = 0;
        _gameSceneManager.LoadScene<BookScene>(gameSceneView);
        LoadCurrent();
        _gameSceneManager.Start();
    }

    public bool Next()
    {
        if (_slides[_index].Next is { } next) return GoTo(next);
        if (_book.IsTurning || _index >= _slides.Count - 1)
        {
            return false;
        }

        _book.CapturePage(backwards: false);
        _index++;
        LoadCurrent();
        return true;
    }

    public bool Previous()
    {
        if (_slides[_index].Previous is { } previous) return GoTo(previous);
        if (_book.IsTurning || _index <= 0)
        {
            return false;
        }

        _book.CapturePage(backwards: true);
        _index--;
        LoadCurrent();
        return true;
    }

    public bool GoTo(PresentationSlide slide)
    {
        if (_book.IsTurning || !_indices.TryGetValue(slide, out var index) || index == _index)
        {
            return false;
        }

        _book.CapturePage(backwards: index < _index);
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

        var scope = _scopeFactory.CreateScope();
        try
        {
            var scene = (GameScene)scope.ServiceProvider.GetRequiredService(definition.SceneType);
            _book.Show(scene, definition.Slide);
            _contentScope?.Dispose();
            _contentScope = scope;
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }
    public void Dispose() => _contentScope?.Dispose();

}
