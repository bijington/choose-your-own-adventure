using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class InputPackageScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    public InputPackageScene(AdventureState state) => _state = state;

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "HOW DOES IT MOVE?",
            "Orbit.Input gives our MAUI game keyboard and game-controller input.");

        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        DrawCodeCard(canvas, new RectF(left.X, left.Y, left.Width, left.Height * .32f),
            "TERMINAL", "dotnet add package Bijington.Orbit.Input");
        DrawCodeCard(canvas, new RectF(left.X, left.Y + left.Height * .38f, left.Width, left.Height * .62f),
            "INPUT", "if (gameController.South.Value)\n{\n    // the player wants to act\n}");
        DrawCodeCard(canvas, right, "MauiProgram.cs",
            "builder\n    .UseMauiApp<App>()\n    .UseOrbitEngine()\n    .UseOrbitGameController()\n    .UseOrbitKeyboard();");

        DrawPrompt(canvas, dimensions, "SOUTH / ENTER  ·  LET’S PUT OUR CHAMPION IN MOTION");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.InputPackage, _state.Champion));
    }
}
