using ChooseYourOwnAdventure.GameObjects;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class OpeningScene : GameScene
{
    private readonly AdventureState _state;
    private double _elapsedMilliseconds;

    public OpeningScene(AdventureState state)
    {
        _state = state;
        Add(new Adventurer(state));
    }

    public override void Update(double millisecondsSinceLastUpdate)
    {
        base.Update(millisecondsSinceLastUpdate);
        _elapsedMilliseconds += millisecondsSinceLastUpdate;
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawBackground(canvas, dimensions);

        if (_state.OpeningStage is OpeningStage.CodeReveal or OpeningStage.Complete)
        {
            DrawCodeRevealShell(canvas, dimensions);
            base.Render(canvas, dimensions);
            DrawCodePanel(canvas, GetCodePanel(dimensions));
            return;
        }

        base.Render(canvas, dimensions);

        switch (_state.OpeningStage)
        {
            case OpeningStage.Title:
                DrawTitle(canvas, dimensions);
                break;
            case OpeningStage.Awakening:
                DrawDialogue(canvas, dimensions, "You awaken in a strange conference hall...");
                break;
            case OpeningStage.Memory:
                DrawDialogue(canvas, dimensions, "You remember nothing.");
                break;
            case OpeningStage.DotNetMemory:
                DrawDialogue(canvas, dimensions, "Except C#.");
                break;
            case OpeningStage.DependencyInjection:
                DrawDialogue(canvas, dimensions, "And, strangely...", "dependency injection.");
                break;
            case OpeningStage.ChooseClass:
                DrawClassSelection(canvas, dimensions);
                break;
            case OpeningStage.ClassChosen:
                DrawClassChosen(canvas, dimensions);
                break;
        }
    }

    private static void DrawBackground(ICanvas canvas, RectF dimensions)
    {
        canvas.FillColor = Color.FromArgb("#0B1020");
        canvas.FillRectangle(dimensions);

        canvas.StrokeColor = Color.FromArgb("#334155");
        canvas.StrokeSize = 2;
        canvas.DrawRectangle(
            dimensions.X + 24,
            dimensions.Y + 24,
            Math.Max(0, dimensions.Width - 48),
            Math.Max(0, dimensions.Height - 48));
    }

    private void DrawTitle(ICanvas canvas, RectF dimensions)
    {
        var width = dimensions.Width;
        var height = dimensions.Height;

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(width * 0.045f, 28f, 64f);
        canvas.DrawString(
            "CHOOSE YOUR OWN ADVENTURE",
            dimensions.X + 40,
            dimensions.Y + (height * 0.28f),
            Math.Max(0, width - 80),
            90,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#CBD5E1");
        canvas.FontSize = Math.Clamp(width * 0.018f, 15f, 26f);
        canvas.DrawString(
            "Built with .NET MAUI + Orbit",
            dimensions.X + 40,
            dimensions.Y + (height * 0.45f),
            Math.Max(0, width - 80),
            50,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        DrawContinuePrompt(canvas, dimensions, "PRESS START", 0.64f);
    }

    private void DrawDialogue(ICanvas canvas, RectF dimensions, string line1, string? line2 = null)
    {
        var width = dimensions.Width;
        var height = dimensions.Height;

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(width * 0.033f, 24f, 48f);
        canvas.DrawString(
            line1,
            dimensions.X + width * 0.10f,
            dimensions.Y + height * 0.37f,
            width * 0.80f,
            70,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        if (!string.IsNullOrWhiteSpace(line2))
        {
            canvas.FontColor = Color.FromArgb("#F8D866");
            canvas.DrawString(
                line2,
                dimensions.X + width * 0.10f,
                dimensions.Y + height * 0.49f,
                width * 0.80f,
                70,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);
        }

        DrawContinuePrompt(canvas, dimensions, "TAP TO CONTINUE", 0.72f);
    }

    private void DrawContinuePrompt(ICanvas canvas, RectF dimensions, string text, float yFactor)
    {
        var pulse = (Math.Sin(_elapsedMilliseconds / 450d) + 1d) / 2d;
        canvas.Alpha = (float)(0.35d + (pulse * 0.65d));
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.018f, 16f, 28f);
        canvas.DrawString(
            text,
            dimensions.X + 40,
            dimensions.Y + (dimensions.Height * yFactor),
            Math.Max(0, dimensions.Width - 80),
            50,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);
        canvas.Alpha = 1f;
    }

    private static void DrawClassSelection(ICanvas canvas, RectF dimensions)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.034f, 26f, 50f);
        canvas.DrawString(
            "CHOOSE YOUR CLASS",
            dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.15f,
            Math.Max(0, dimensions.Width - 80),
            70,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        var cardTop = dimensions.Y + dimensions.Height * 0.34f;
        var cardHeight = dimensions.Height * 0.35f;
        var gap = Math.Max(18f, dimensions.Width * 0.018f);
        var totalWidth = dimensions.Width * 0.84f;
        var cardWidth = (totalWidth - gap * 2) / 3f;
        var startX = dimensions.X + (dimensions.Width - totalWidth) / 2f;

        DrawClassCard(canvas, new RectF(startX, cardTop, cardWidth, cardHeight), "MAUI", "WARRIOR", "⚔");
        DrawClassCard(canvas, new RectF(startX + cardWidth + gap, cardTop, cardWidth, cardHeight), "ASYNC", "WIZARD", "✦");
        DrawClassCard(canvas, new RectF(startX + (cardWidth + gap) * 2, cardTop, cardWidth, cardHeight), "XAML", "RANGER", "⌁");

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.014f, 13f, 22f);
        canvas.DrawString(
            "Tap a class to choose your adventurer",
            dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.77f,
            Math.Max(0, dimensions.Width - 80),
            40,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);
    }

    private static void DrawClassCard(ICanvas canvas, RectF rect, string line1, string line2, string glyph)
    {
        canvas.FillColor = Color.FromArgb("#111827");
        canvas.FillRoundedRectangle(rect, 18);
        canvas.StrokeColor = Color.FromArgb("#475569");
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 18);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#F8D866");
        canvas.FontSize = Math.Clamp(rect.Width * 0.22f, 24f, 54f);
        canvas.DrawString(glyph, rect.X, rect.Y + rect.Height * 0.11f, rect.Width, rect.Height * 0.28f,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(rect.Width * 0.10f, 15f, 25f);
        canvas.DrawString(line1, rect.X + 8, rect.Y + rect.Height * 0.50f, rect.Width - 16, 32,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.DrawString(line2, rect.X + 8, rect.Y + rect.Height * 0.63f, rect.Width - 16, 32,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawClassChosen(ICanvas canvas, RectF dimensions)
    {
        var chosen = GetClassName(_state.PlayerClass);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#F8D866");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.042f, 30f, 58f);
        canvas.DrawString(
            chosen.ToUpperInvariant(),
            dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.30f,
            Math.Max(0, dimensions.Width - 80),
            80,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.024f, 20f, 34f);
        canvas.DrawString(
            "Your adventure begins.",
            dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.46f,
            Math.Max(0, dimensions.Width - 80),
            60,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        DrawContinuePrompt(canvas, dimensions, "SHOW ME THE CODE", 0.68f);
    }

    private static RectF GetCodePanel(RectF dimensions)
    {
        var margin = Math.Max(42f, dimensions.Width * 0.05f);
        var gap = Math.Max(28f, dimensions.Width * 0.035f);
        var contentTop = dimensions.Y + dimensions.Height * 0.16f;
        var contentHeight = dimensions.Height * 0.66f;
        var panelWidth = (dimensions.Width - (margin * 2) - gap) / 2f;

        return new RectF(dimensions.X + margin, contentTop, panelWidth, contentHeight);
    }

    private static void DrawCodeRevealShell(ICanvas canvas, RectF dimensions)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.028f, 24f, 42f);
        canvas.DrawString(
            "REAL ORBIT CODE → LIVE GAMEOBJECT",
            dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.055f,
            Math.Max(0, dimensions.Width - 80),
            60,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        var left = GetCodePanel(dimensions);
        canvas.FillColor = Color.FromArgb("#0F172A");
        canvas.FillRoundedRectangle(left, 18);
        canvas.StrokeColor = Color.FromArgb("#334155");
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(left, 18);
    }

    private void DrawCodePanel(ICanvas canvas, RectF rect)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = Math.Clamp(rect.Width * 0.042f, 13f, 20f);
        canvas.DrawString("OpeningScene.cs", rect.X + 24, rect.Y + 14, rect.Width - 48, 26,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        string[] sceneLines =
        [
            "// OpeningScene",
            "Add(new Adventurer(state));",
            "",
            "public sealed class Adventurer : GameObject",
            "{",
            "    private readonly AdventureState _state;",
            "",
            "    public Adventurer(AdventureState state)",
            "        => _state = state;",
            "",
            "    public override void Render(",
            "        ICanvas canvas, RectF dimensions)",
            "    {",
            "        if (_state.OpeningStage is not",
            "            (OpeningStage.CodeReveal or",
            "             OpeningStage.Complete)) return;",
            "",
            "        DrawHero(canvas, dimensions,",
            "            _state.PlayerClass);",
            "    }",
            "}"
        ];

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#E2E8F0");
        canvas.FontSize = Math.Clamp(rect.Width * 0.027f, 10f, 15f);

        var lineHeight = Math.Clamp(rect.Height * 0.037f, 17f, 23f);
        var y = rect.Y + rect.Height * 0.10f;
        foreach (var line in sceneLines)
        {
            canvas.DrawString(line, rect.X + 24, y, rect.Width - 48, lineHeight,
                HorizontalAlignment.Left, VerticalAlignment.Center);
            y += lineHeight;
        }

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#F8D866");
        canvas.FontSize = Math.Clamp(rect.Width * 0.031f, 11f, 16f);
        canvas.DrawString(
            $"Selected: {GetClassName(_state.PlayerClass)}",
            rect.X + 24,
            rect.Bottom - 40,
            rect.Width - 48,
            24,
            HorizontalAlignment.Left,
            VerticalAlignment.Center);
    }

    private static string GetClassName(PlayerClass? playerClass) => playerClass switch
    {
        PlayerClass.MauiWarrior => "MAUI Warrior",
        PlayerClass.XamlRanger => "XAML Ranger",
        _ => "Async Wizard"
    };
}
