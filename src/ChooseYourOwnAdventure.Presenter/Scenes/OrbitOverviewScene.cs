using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class OrbitOverviewScene : PresentationSceneBase
{
    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "WHAT IS ORBIT?",
            "A lightweight game engine built on .NET MAUI Graphics — game-oriented concepts without leaving MAUI.");

        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .019f, 17f, 28f);
        canvas.DrawString("A game engine inside a MAUI app.\n\nScenes hold the world.\nGameObjects bring it to life.\n\nUpdate changes state.\nRender makes it visible.",
            left.X, left.Y + 20, left.Width, left.Height,
            HorizontalAlignment.Left, VerticalAlignment.Top);
        var w = Math.Min(340f, right.Width * .85f);
        var h = 76f;
        var x = right.X + (right.Width - w) / 2f;
        var y = right.Y;
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
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.OrbitOverview));
    }
}
