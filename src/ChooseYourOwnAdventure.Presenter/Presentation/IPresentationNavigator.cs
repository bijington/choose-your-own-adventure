using Orbit.Engine;

namespace ChooseYourOwnAdventure.Presentation;

public interface IPresentationNavigator
{
    PresentationSlide Current { get; }

    void Start(GameSceneView gameSceneView);

    bool Next();

    bool Previous();

    bool GoTo(PresentationSlide slide);
}
