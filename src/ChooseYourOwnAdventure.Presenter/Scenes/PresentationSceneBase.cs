using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

public abstract class PresentationSceneBase : GameScene
{
    protected static readonly Color Background = PresentationTheme.Parchment;
    protected static readonly Color Panel = PresentationTheme.CodePanel;
    protected static readonly Color Border = PresentationTheme.CodePanelBorder;
    protected static readonly Color Accent = PresentationTheme.AccentRed;
    protected static readonly Color Gold = PresentationTheme.AccentGold;
    protected static readonly Color Muted = PresentationTheme.MutedInk;
    protected static readonly Color Ink = PresentationTheme.Ink;

    protected static void DrawShell(ICanvas canvas, RectF dimensions)
    {
        PresentationTheme.DrawParchment(canvas, dimensions);
    }

    protected static void DrawTitle(ICanvas canvas, RectF dimensions, string title, string? subtitle = null)
    {
        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.025f, 24f, 38f);
        canvas.DrawString(title, left.X, dimensions.Y + dimensions.Height * .045f,
            left.Width, dimensions.Height * .14f, HorizontalAlignment.Left, VerticalAlignment.Center);
        canvas.StrokeColor = Accent;
        canvas.StrokeSize = 2;
        var ruleY = dimensions.Y + dimensions.Height * .205f;
        canvas.DrawLine(left.X, ruleY, left.Right, ruleY);
        canvas.DrawLine(right.X, ruleY, right.Right, ruleY);

        if (string.IsNullOrWhiteSpace(subtitle)) return;
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * .015f, 14f, 22f);
        canvas.DrawString(subtitle, right.X, dimensions.Y + dimensions.Height * .06f,
            right.Width, dimensions.Height * .13f, HorizontalAlignment.Left, VerticalAlignment.Center);
    }

    protected static void DrawCodeCard(ICanvas canvas, RectF rect, string title, string code)
    {
        canvas.FillColor = PresentationTheme.CodePanel;
        canvas.FillRoundedRectangle(rect, 18);
        canvas.StrokeColor = PresentationTheme.CodePanelBorder;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 18);

        var header = new RectF(rect.X, rect.Y, rect.Width, 44);
        canvas.FillColor = PresentationTheme.CodePanelHeader;
        canvas.FillRoundedRectangle(header, 18);
        canvas.FillRectangle(rect.X, rect.Y + 22, rect.Width, 22);

        canvas.Font = CodeFonts.SemiBold;
        canvas.FontColor = PresentationTheme.AccentRed;
        canvas.FontSize = Math.Clamp(rect.Width * 0.040f, 16f, 24f);
        canvas.DrawString(title, rect.X + 22, rect.Y + 2, rect.Width - 44, 42,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = CodeFonts.Regular;
        canvas.FontColor = PresentationTheme.CodeText;
        var lines = code.Split('\n');
        var heightLimit = Math.Max(10f, (rect.Height - 76f) / (lines.Length * 1.35f));
        var widthLimit = (rect.Width - 44f) / (Math.Max(1, lines.Max(line => line.Length)) * 0.64f);
        canvas.FontSize = Math.Min(Math.Clamp(rect.Width * 0.034f, 16f, 24f), Math.Min(heightLimit, widthLimit));
        canvas.DrawString(code, rect.X + 22, rect.Y + 60, rect.Width - 44, rect.Height - 80,
            HorizontalAlignment.Left, VerticalAlignment.Top);
    }

    protected static void DrawPrompt(ICanvas canvas, RectF dimensions, string prompt = "SOUTH / ENTER  ·  CONTINUE")
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.012f, 11f, 17f);
        canvas.DrawString(prompt, BookLayout.LeftPage(dimensions).X, dimensions.Bottom - 90,
            BookLayout.LeftPage(dimensions).Width, 52, HorizontalAlignment.Left, VerticalAlignment.Center);
    }


    protected static void DrawPageNumber(ICanvas canvas, RectF dimensions, int pageNumber, string label = "PAGE")
    {
        PresentationTheme.DrawPageNumber(canvas, dimensions, pageNumber, label);
    }

    protected static void DrawPill(ICanvas canvas, RectF rect, string text, bool accent = false)
    {
        canvas.FillColor = accent ? Color.FromArgb("#F9D8C8") : Color.FromArgb("#FFF4D8");
        canvas.FillRoundedRectangle(rect, 14);
        canvas.StrokeColor = accent ? Accent : PresentationTheme.Rule;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 14);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = accent ? Accent : Ink;
        canvas.FontSize = Math.Clamp(rect.Width * 0.045f, 13f, 20f);
        canvas.DrawString(text, rect.X + 14, rect.Y + 8, rect.Width - 28, rect.Height - 16,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}
