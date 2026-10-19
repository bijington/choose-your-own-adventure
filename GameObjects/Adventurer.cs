using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.GameObjects;

public sealed class Adventurer : GameObject
{
    private readonly AdventureState _state;

    public Adventurer(AdventureState state)
    {
        _state = state;
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        base.Render(canvas, dimensions);

        if (_state.OpeningStage is not (OpeningStage.CodeReveal or OpeningStage.Complete))
        {
            return;
        }

        var rect = GetSpritePanel(dimensions);
        DrawPanel(canvas, rect);
        DrawHero(canvas, rect, _state.PlayerClass);
    }

    private static RectF GetSpritePanel(RectF dimensions)
    {
        var margin = Math.Max(42f, dimensions.Width * 0.05f);
        var gap = Math.Max(28f, dimensions.Width * 0.035f);
        var contentTop = dimensions.Y + dimensions.Height * 0.16f;
        var contentHeight = dimensions.Height * 0.66f;
        var panelWidth = (dimensions.Width - (margin * 2) - gap) / 2f;
        var left = new RectF(dimensions.X + margin, contentTop, panelWidth, contentHeight);

        return new RectF(left.Right + gap, contentTop, panelWidth, contentHeight);
    }

    private static void DrawPanel(ICanvas canvas, RectF rect)
    {
        canvas.FillColor = Color.FromArgb("#111827");
        canvas.FillRoundedRectangle(rect, 18);
        canvas.StrokeColor = Color.FromArgb("#334155");
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 18);
    }

    private static void DrawHero(ICanvas canvas, RectF rect, PlayerClass? playerClass)
    {
        var name = playerClass switch
        {
            PlayerClass.MauiWarrior => "MAUI Warrior",
            PlayerClass.XamlRanger => "XAML Ranger",
            _ => "Async Wizard"
        };

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(rect.Width * 0.064f, 18f, 30f);
        canvas.DrawString(
            name,
            rect.X + 20,
            rect.Y + 24,
            rect.Width - 40,
            40,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        var centerX = rect.X + rect.Width / 2f;
        var groundY = rect.Y + rect.Height * 0.77f;
        DrawPixelHero(canvas, centerX, groundY, Math.Clamp(rect.Width * 0.045f, 10f, 18f), playerClass);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = Math.Clamp(rect.Width * 0.038f, 12f, 18f);
        canvas.DrawString(
            "Rendered by this Orbit GameObject",
            rect.X + 20,
            rect.Bottom - 58,
            rect.Width - 40,
            32,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);
    }

    private static void DrawPixelHero(
        ICanvas canvas,
        float centerX,
        float groundY,
        float p,
        PlayerClass? playerClass)
    {
        var primary = playerClass switch
        {
            PlayerClass.MauiWarrior => Color.FromArgb("#38BDF8"),
            PlayerClass.XamlRanger => Color.FromArgb("#34D399"),
            _ => Color.FromArgb("#A78BFA")
        };

        var accent = playerClass switch
        {
            PlayerClass.MauiWarrior => Color.FromArgb("#E2E8F0"),
            PlayerClass.XamlRanger => Color.FromArgb("#F8D866"),
            _ => Color.FromArgb("#F8D866")
        };

        void Pixel(float x, float y, float w, float h, Color color)
        {
            canvas.FillColor = color;
            canvas.FillRectangle(centerX + x * p, groundY + y * p, w * p, h * p);
        }

        Pixel(-2.5f, 0.2f, 5f, 0.5f, Color.FromArgb("#020617"));
        Pixel(-1.7f, -2f, 1.3f, 2f, Color.FromArgb("#334155"));
        Pixel(0.4f, -2f, 1.3f, 2f, Color.FromArgb("#334155"));
        Pixel(-2f, -5.5f, 4f, 3.5f, primary);
        Pixel(-3f, -5f, 1f, 3f, primary);
        Pixel(2f, -5f, 1f, 3f, primary);
        Pixel(-1.5f, -8.5f, 3f, 3f, Color.FromArgb("#F1C7A5"));
        Pixel(-1.8f, -9.2f, 3.6f, 1f, accent);

        if (playerClass == PlayerClass.MauiWarrior)
        {
            Pixel(3.2f, -7f, 0.5f, 5.5f, accent);
            Pixel(2.7f, -7.5f, 1.5f, 0.5f, accent);
        }
        else if (playerClass == PlayerClass.XamlRanger)
        {
            Pixel(3f, -6.8f, 0.45f, 5f, accent);
            Pixel(3.45f, -6.3f, 0.35f, 4f, accent);
        }
        else
        {
            Pixel(-2.3f, -10.3f, 4.6f, 1f, accent);
            Pixel(-1.2f, -11.3f, 2.4f, 1f, accent);
            Pixel(3.3f, -5.5f, 0.5f, 5f, accent);
        }
    }
}
