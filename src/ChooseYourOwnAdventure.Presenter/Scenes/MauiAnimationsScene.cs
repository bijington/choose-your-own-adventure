using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class MauiAnimationsScene : PresentationSceneBase
{
    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "MAUI ALREADY HAS SOME MAGIC",
            "Real MAUI controls. Real MAUI animations. A little personality before we meet Orbit.");
        var left = BookLayout.LeftPage(dimensions);
        DrawCodeCard(canvas, new RectF(left.X, left.Y, left.Width, left.Height * .67f),
            "C# · animate a MAUI view", """
new Animation(t =>
{
    view.Rotation = 360 *
        Easing.CubicInOut.Ease(t);
    view.Scale = 1 + .16 *
        Math.Sin(t * Math.PI * 2);
}).Commit(this, "Showtime",
    length: 2400,
    repeat: () => IsVisible);
""");
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .016f, 16f, 24f);
        canvas.DrawString("Great for transitions, feedback and playful UI.\nNext: a shared game loop, scenes and game objects.",
            left.X, left.Y + left.Height * .74f, left.Width, left.Height * .26f,
            HorizontalAlignment.Left, VerticalAlignment.Top);
        DrawPrompt(canvas, dimensions, "SOUTH / RB / ENTER / PGDN · BUILDING A GAME IN MAUI");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.MauiAnimations));
    }
}
