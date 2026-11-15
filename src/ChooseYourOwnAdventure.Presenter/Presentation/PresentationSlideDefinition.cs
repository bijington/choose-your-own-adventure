using Orbit.Engine;
using System.Diagnostics.CodeAnalysis;

namespace ChooseYourOwnAdventure.Presentation;

public sealed record PresentationSlideDefinition(
    PresentationSlide Slide,
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    Type SceneType,
    Action<AdventureState>? OnEnter = null)
{
    public static PresentationSlideDefinition Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TScene>(
        PresentationSlide slide,
        Action<AdventureState>? onEnter = null)
        where TScene : GameScene
        => new(slide, typeof(TScene), onEnter);
}
