using Orbit.Engine;

namespace ChooseYourOwnAdventure.Presentation;

public sealed record PresentationSlideDefinition(
    PresentationSlide Slide,
    Type SceneType,
    Action<AdventureState>? OnEnter = null)
{
    public static PresentationSlideDefinition Create<TScene>(
        PresentationSlide slide,
        Action<AdventureState>? onEnter = null)
        where TScene : GameScene
        => new(slide, typeof(TScene), onEnter);
}
