using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class FutureIdeasScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    public FutureIdeasScene(AdventureState state) => _state = state;

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "WHAT WOULD WE\nADD NEXT?",
            "How do you know your boost landed?");

        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        var top = dimensions.Y + dimensions.Height * .29f;
        var rowHeight = dimensions.Height * .22f;

        DrawIdea(canvas, new RectF(left.X, top, left.Width, rowHeight),
            "HAPTICS", "A short pulse when you tap Boost");
        DrawIdea(canvas, new RectF(left.X, top + rowHeight, left.Width, rowHeight),
            "AUDIO", "Countdown beeps and a finish sound");
        DrawIdea(canvas, new RectF(right.X, top, right.Width, rowHeight),
            "VISUAL FEEDBACK", "Highlight Boost on tap.\nKeep the countdown visible.");

        DrawPrompt(canvas, dimensions, "SOUTH / ENTER  ·  ADVENTURE COMPLETE");
        DrawPageNumber(canvas, dimensions,
            PresentationTheme.GetPageNumber(PresentationSlide.FutureIdeas, _state.Champion));
    }

    private static void DrawIdea(ICanvas canvas, RectF bounds, string title, string description)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(bounds.Width * .055f, 15f, 28f);
        canvas.DrawString(title, bounds.X, bounds.Y, bounds.Width, bounds.Height * .26f,
            HorizontalAlignment.Left, VerticalAlignment.Top);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(bounds.Width * .048f, 14f, 24f);
        canvas.DrawString(description, bounds.X, bounds.Y + bounds.Height * .26f,
            bounds.Width, bounds.Height * .62f, HorizontalAlignment.Left, VerticalAlignment.Top);
    }
}
