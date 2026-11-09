using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

public abstract class PresentationSceneBase : GameScene
{
    protected static readonly Color Background = Color.FromArgb("#07111F");
    protected static readonly Color Panel = Color.FromArgb("#0F172A");
    protected static readonly Color Border = Color.FromArgb("#334155");
    protected static readonly Color Accent = Color.FromArgb("#F8D866");
    protected static readonly Color Muted = Color.FromArgb("#94A3B8");

    protected static void DrawShell(ICanvas canvas, RectF dimensions)
    {
        canvas.FillColor = Background;
        canvas.FillRectangle(dimensions);
        canvas.StrokeColor = Color.FromArgb("#26364D");
        canvas.StrokeSize = 2;
        canvas.DrawRectangle(dimensions.X + 24, dimensions.Y + 24,
            Math.Max(0, dimensions.Width - 48), Math.Max(0, dimensions.Height - 48));
    }

    protected static void DrawTitle(ICanvas canvas, RectF dimensions, string title, string? subtitle = null)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.036f, 28f, 54f);
        canvas.DrawString(title, dimensions.X + 52, dimensions.Y + dimensions.Height * 0.06f,
            dimensions.Width - 104, 58, HorizontalAlignment.Center, VerticalAlignment.Center);

        if (string.IsNullOrWhiteSpace(subtitle))
        {
            return;
        }

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.015f, 13f, 22f);
        canvas.DrawString(subtitle, dimensions.X + 70, dimensions.Y + dimensions.Height * 0.135f,
            dimensions.Width - 140, 40, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    protected static void DrawCodeCard(ICanvas canvas, RectF rect, string title, string code)
    {
        canvas.FillColor = Panel;
        canvas.FillRoundedRectangle(rect, 18);
        canvas.StrokeColor = Border;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 18);

        canvas.Font = CodeFonts.SemiBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(rect.Width * 0.038f, 13f, 20f);
        canvas.DrawString(title, rect.X + 22, rect.Y + 16, rect.Width - 44, 28,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = CodeFonts.Regular;
        canvas.FontColor = Color.FromArgb("#E2E8F0");
        canvas.FontSize = Math.Clamp(rect.Width * 0.030f, 12f, 18f);
        canvas.DrawString(code, rect.X + 22, rect.Y + 56, rect.Width - 44, rect.Height - 76,
            HorizontalAlignment.Left, VerticalAlignment.Top);
    }

    protected static void DrawPrompt(ICanvas canvas, RectF dimensions, string prompt = "A / ENTER  ·  CONTINUE")
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#64748B");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.012f, 11f, 17f);
        canvas.DrawString(prompt, dimensions.X + 50, dimensions.Bottom - 56,
            dimensions.Width - 100, 24, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    protected static void DrawPill(ICanvas canvas, RectF rect, string text, bool accent = false)
    {
        canvas.FillColor = accent ? Color.FromArgb("#2A2411") : Panel;
        canvas.FillRoundedRectangle(rect, 14);
        canvas.StrokeColor = accent ? Accent : Border;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 14);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = accent ? Accent : Colors.White;
        canvas.FontSize = Math.Clamp(rect.Width * 0.045f, 13f, 20f);
        canvas.DrawString(text, rect.X + 14, rect.Y + 8, rect.Width - 28, rect.Height - 16,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}
