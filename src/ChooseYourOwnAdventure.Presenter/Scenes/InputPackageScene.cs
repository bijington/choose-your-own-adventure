using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class InputPackageScene : PresentationSceneBase
{
    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "HOW DOES IT MOVE?",
            "Orbit.Input gives our MAUI game keyboard and game-controller input.");

        var margin = dimensions.Width * 0.08f;
        var width = dimensions.Width - margin * 2;
        var first = new RectF(dimensions.X + margin, dimensions.Y + dimensions.Height * 0.23f, width, 120);
        DrawCodeCard(canvas, first, "TERMINAL", "dotnet add package Bijington.Orbit.Input");

        var second = new RectF(dimensions.X + margin, first.Bottom + 28, width, 210);
        DrawCodeCard(canvas, second, "MauiProgram.cs",
            "builder\n    .UseMauiApp<App>()\n    .UseOrbitEngine()\n    .UseOrbitGameController()\n    .UseOrbitKeyboard();");

        var third = new RectF(dimensions.X + margin, second.Bottom + 28, width, 150);
        DrawCodeCard(canvas, third, "INPUT",
            "if (gameController.ButtonSouth.Value)\n{\n    // the player wants to do something\n}");

        DrawPrompt(canvas, dimensions, "A / ENTER  ·  WHERE DOES THAT INPUT GET PROCESSED?");
    }
}
