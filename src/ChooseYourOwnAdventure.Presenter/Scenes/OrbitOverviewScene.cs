using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class OrbitOverviewScene : PresentationSceneBase
{
    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "WHAT IS ORBIT?",
            "A lightweight game engine built on .NET MAUI Graphics — game-oriented concepts without leaving MAUI.");

        var w = Math.Min(300f, dimensions.Width * 0.22f);
        var h = 76f;
        var x = dimensions.X + (dimensions.Width - w) / 2f;
        var y = dimensions.Y + dimensions.Height * 0.27f;
        var labels = new[] { ".NET MAUI", "MAUI GRAPHICS", "ORBIT", "YOUR GAME" };

        for (var i = 0; i < labels.Length; i++)
        {
            var rect = new RectF(x, y + i * 108f, w, h);
            DrawPill(canvas, rect, labels[i], i == 2);
            if (i < labels.Length - 1)
            {
                canvas.StrokeColor = Muted;
                canvas.StrokeSize = 2;
                canvas.DrawLine(x + w / 2f, rect.Bottom + 7, x + w / 2f, rect.Bottom + 30);
                canvas.DrawLine(x + w / 2f - 7, rect.Bottom + 23, x + w / 2f, rect.Bottom + 30);
                canvas.DrawLine(x + w / 2f + 7, rect.Bottom + 23, x + w / 2f, rect.Bottom + 30);
            }
        }

        DrawPrompt(canvas, dimensions);
    }
}
