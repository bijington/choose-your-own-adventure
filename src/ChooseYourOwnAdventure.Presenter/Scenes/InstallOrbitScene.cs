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

        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        DrawCodeCard(canvas, new RectF(left.X, left.Y, left.Width, left.Height * .34f),
            "TERMINAL", "dotnet add package Bijington.Orbit.Engine");
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .018f, 16f, 26f);
        canvas.DrawString("First, add Orbit to your MAUI app.\n\nThen register the engine when you build the application.",
            left.X + 12, left.Y + left.Height * .44f, left.Width - 24, left.Height * .46f,
            HorizontalAlignment.Left, VerticalAlignment.Top);
        DrawCodeCard(canvas, right, "MauiProgram.cs",
            "builder\n    .UseMauiApp<App>()\n    .UseOrbitEngine();");

        DrawPrompt(canvas, dimensions);
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.InstallOrbit));
    }
}
