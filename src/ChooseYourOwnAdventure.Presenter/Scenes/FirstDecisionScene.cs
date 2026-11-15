using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class FirstDecisionScene : PresentationSceneBase
{
    private static RectF AfterPartyChoice(RectF bounds)
    {
        var right = BookLayout.RightPage(bounds);
        return new RectF(right.X, right.Y + right.Height * .53f, right.Width, right.Height * .43f);
    }

    public static bool IsAfterPartyChoice(RectF bounds, PointF point) => AfterPartyChoice(bounds).Contains(point);

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "YOUR FIRST DECISION",
            "One final session. Two possible paths. What does the room choose?");
        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .019f, 18f, 28f);
        canvas.DrawString("Your final session awaits.\n\nBeyond the doors, the after-party beckons.\n\nDo you begin an adventure…\nor follow the sound of the party?",
            left.X, left.Y + 12, left.Width, left.Height,
            HorizontalAlignment.Left, VerticalAlignment.Top);
        DrawChoice(canvas, new RectF(right.X, right.Y, right.Width, right.Height * .43f),
            "ATTEND THE TALK", "TURN TO PAGE 2");
        DrawChoice(canvas, AfterPartyChoice(dimensions),
            "GO STRAIGHT TO THE AFTER-PARTY", "TURN TO PAGE 404");
        DrawPrompt(canvas, dimensions, "A / ENTER · THE TALK    B / SHIFT · THE AFTER-PARTY");
        DrawPageNumber(canvas, dimensions, 1);
    }

    private static void DrawChoice(ICanvas canvas, RectF card, string title, string destination)
    {
        canvas.FillColor = Panel;
        canvas.FillRoundedRectangle(card, 16);
        canvas.StrokeColor = Border;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(card, 16);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(card.Width * .05f, 18f, 30f);
        canvas.DrawString(title, card.X + 22, card.Y + 12, card.Width - 44, card.Height * .53f,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(card.Width * .045f, 17f, 28f);
        canvas.DrawString(destination, card.X + 22, card.Y + card.Height * .60f,
            card.Width - 44, card.Height * .30f, HorizontalAlignment.Left, VerticalAlignment.Center);
    }
}
