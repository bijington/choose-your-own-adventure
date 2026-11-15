using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class AddGameObjectScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    public AddGameObjectScene(AdventureState state)
    {
        _state = state;
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "ADD A GAMEOBJECT",
            "A scene contains the things in our world. Our champion becomes a real Orbit GameObject.");

        var pageNumber = PresentationTheme.GetPageNumber(PresentationSlide.AddGameObject, _state.Champion);

        var margin = dimensions.Width * 0.07f;
        var gap = dimensions.Width * 0.035f;
        var width = (dimensions.Width - margin * 2 - gap) / 2f;
        var top = dimensions.Y + dimensions.Height * 0.24f;
        var height = dimensions.Height * 0.55f;

        DrawCodeCard(canvas, new RectF(dimensions.X + margin, top, width, height),
            "ChampionGameObject.cs",
            """
public sealed class ChampionGameObject
    : GameObject
{
    public override void Render(
        ICanvas canvas,
        RectF dimensions)
    {
        // draw the champion
    }
}
""");

        DrawCodeCard(canvas, new RectF(dimensions.X + margin + width + gap, top, width, height),
            "GameScene",
            "Add(new ChampionGameObject(state));");

        DrawPrompt(canvas, dimensions, "A / ENTER  ·  ADD OUR CHAMPION");
        DrawPageNumber(canvas, dimensions, pageNumber);
    }
}
