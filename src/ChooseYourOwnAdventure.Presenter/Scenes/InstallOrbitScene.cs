using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class InstallOrbitScene : PresentationSceneBase
{
    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "INSTALL & REGISTER ORBIT",
            "It is still a normal .NET MAUI app — Orbit arrives as NuGet packages and builder extensions.");

        var margin = dimensions.Width * 0.10f;
        var width = dimensions.Width - margin * 2;
        var first = new RectF(dimensions.X + margin, dimensions.Y + dimensions.Height * 0.26f, width, 150);
        DrawCodeCard(canvas, first, "TERMINAL", "dotnet add package Bijington.Orbit.Engine");

        var second = new RectF(dimensions.X + margin, first.Bottom + 36, width, 285);
        DrawCodeCard(canvas, second, "MauiProgram.cs",
            "builder\n    .UseMauiApp<App>()\n    .UseOrbitEngine();");

        DrawPrompt(canvas, dimensions);
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.InstallOrbit));
    }
}
