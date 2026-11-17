using Microsoft.Maui.Graphics;
using ChooseYourOwnAdventure.Presentation;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class AfterPartyScene : PresentationSceneBase
{
    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "A PROMISING SHORTCUT…", "You turn eagerly to page 404.");
        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * .09f, 64f, 130f);
        canvas.DrawString("404", left.X, left.Y, left.Width, left.Height * .48f,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.FontSize = Math.Clamp(dimensions.Width * .02f, 19f, 30f);
        canvas.DrawString("AFTER-PARTY NOT FOUND", left.X, left.Y + left.Height * .5f,
            left.Width, left.Height * .22f, HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .021f, 20f, 30f);
        canvas.DrawString("The after-party could not be found.\n\nPlease complete this adventure first.",
            right.X, right.Y + 20, right.Width, right.Height * .65f,
            HorizontalAlignment.Left, VerticalAlignment.Top);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * .018f, 18f, 27f);
        canvas.DrawString("TURN TO PAGE 2", right.X, right.Y + right.Height * .73f,
            right.Width, 64, HorizontalAlignment.Left, VerticalAlignment.Center);
        DrawPrompt(canvas, dimensions, "SOUTH / ENTER · BEGIN THE ADVENTURE");
        DrawPageNumber(canvas, dimensions, 404);
    }
}
