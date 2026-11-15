using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class SignalRTransitionScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    public SignalRTransitionScene(AdventureState state) => _state = state;

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "WHAT IF THE INPUT SOURCE CHANGES?",
            "The game logic does not need to care whether intent came from one controller… or an entire room.");

        var y = dimensions.Y + dimensions.Height * 0.36f;
        var boxW = Math.Min(280f, dimensions.Width * 0.24f);
        var boxH = 120f;
        var left = new RectF(dimensions.X + dimensions.Width * 0.15f, y, boxW, boxH);
        var right = new RectF(dimensions.Right - dimensions.Width * 0.15f - boxW, y, boxW, boxH);
        DrawPill(canvas, left, "🎮 LOCAL CONTROLLER", true);
        DrawPill(canvas, right, "📱📱📱 THE AUDIENCE", true);

        canvas.StrokeColor = Accent;
        canvas.StrokeSize = 3;
        var centreY = y + boxH / 2f;
        canvas.DrawLine(left.Right + 25, centreY, right.X - 25, centreY);
        canvas.DrawLine(right.X - 37, centreY - 8, right.X - 25, centreY);
        canvas.DrawLine(right.X - 37, centreY + 8, right.X - 25, centreY);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.025f, 20f, 36f);
        canvas.DrawString("Same game. Different source of intent.", dimensions.X + 60,
            dimensions.Y + dimensions.Height * 0.64f, dimensions.Width - 120, 52,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        DrawPrompt(canvas, dimensions, "A / ENTER  ·  REVEAL HOW THE OPENING VOTE WORKED");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.SignalRTransition, _state.Champion));
    }
}
