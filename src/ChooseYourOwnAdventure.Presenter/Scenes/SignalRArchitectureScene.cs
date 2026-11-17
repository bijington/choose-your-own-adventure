using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class SignalRArchitectureScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    public SignalRArchitectureScene(AdventureState state) => _state = state;

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "SIGNALR: THE ROOM BECOMES THE INPUT",
            "You have actually been using this architecture since the champion vote.");

        var labels = new[] { "AUDIENCE BROWSERS", "SIGNALR HUB", "SERVER-OWNED STATE", "MAUI + ORBIT" };
        var boxW = Math.Min(245f, dimensions.Width * 0.20f);
        var boxH = 100f;
        var gap = Math.Min(42f, dimensions.Width * 0.035f);
        var total = labels.Length * boxW + (labels.Length - 1) * gap;
        var x = dimensions.X + (dimensions.Width - total) / 2f;
        var y = dimensions.Y + dimensions.Height * 0.38f;

        for (var i = 0; i < labels.Length; i++)
        {
            var rect = new RectF(x + i * (boxW + gap), y, boxW, boxH);
            DrawPill(canvas, rect, labels[i], i == 1 || i == 2);
            if (i < labels.Length - 1)
            {
                var lineX1 = rect.Right + 8;
                var lineX2 = rect.Right + gap - 8;
                var lineY = rect.Y + rect.Height / 2f;
                canvas.StrokeColor = Muted;
                canvas.StrokeSize = 2;
                canvas.DrawLine(lineX1, lineY, lineX2, lineY);
                canvas.DrawLine(lineX2 - 8, lineY - 6, lineX2, lineY);
                canvas.DrawLine(lineX2 - 8, lineY + 6, lineX2, lineY);
            }
        }

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.018f, 15f, 26f);
        canvas.DrawString("Browser sends intent  •  server owns shared state  •  Orbit renders the result",
            dimensions.X + 60, dimensions.Y + dimensions.Height * 0.63f, dimensions.Width - 120, 42,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        DrawPrompt(canvas, dimensions, "SOUTH / ENTER  ·  GIVE CONTROL TO THE ROOM");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.SignalRArchitecture, _state.Champion));
    }
}
