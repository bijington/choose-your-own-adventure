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
        Add(new ChampionGameObject(state));
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
            DrawContinuePrompt(canvas, dimensions, "A / ENTER  ·  CONTINUE TO THE GAME LOOP", 0.88f);
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
            case OpeningStage.ChooseChampion:
                DrawChampionSelection(canvas, dimensions, _state.HighlightedChampion);
                break;
            case OpeningStage.ChampionChosen:
                DrawChampionChosen(canvas, dimensions);
                break;
        }
    }

    private static void DrawBackground(ICanvas canvas, RectF dimensions)
    {
        canvas.FillColor = Color.FromArgb("#0B1020");
        canvas.FillRectangle(dimensions);
        canvas.StrokeColor = Color.FromArgb("#334155");
        canvas.StrokeSize = 2;
        canvas.DrawRectangle(dimensions.X + 24, dimensions.Y + 24,
            Math.Max(0, dimensions.Width - 48), Math.Max(0, dimensions.Height - 48));
    }

    private void DrawTitle(ICanvas canvas, RectF dimensions)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.045f, 28f, 64f);
        canvas.DrawString("CHOOSE YOUR OWN ADVENTURE", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.28f, Math.Max(0, dimensions.Width - 80), 90,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#CBD5E1");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.018f, 15f, 26f);
        canvas.DrawString("Built with .NET MAUI + Orbit", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.45f, Math.Max(0, dimensions.Width - 80), 50,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        DrawContinuePrompt(canvas, dimensions, "PRESS START", 0.64f);
    }

    private void DrawDialogue(ICanvas canvas, RectF dimensions, string line1, string? line2 = null)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.033f, 24f, 48f);
        canvas.DrawString(line1, dimensions.X + dimensions.Width * 0.10f,
            dimensions.Y + dimensions.Height * 0.37f, dimensions.Width * 0.80f, 70,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        if (!string.IsNullOrWhiteSpace(line2))
        {
            canvas.FontColor = Color.FromArgb("#F8D866");
            canvas.DrawString(line2, dimensions.X + dimensions.Width * 0.10f,
                dimensions.Y + dimensions.Height * 0.49f, dimensions.Width * 0.80f, 70,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        DrawContinuePrompt(canvas, dimensions, "A / ENTER  ·  CONTINUE", 0.72f);
    }

    private void DrawContinuePrompt(ICanvas canvas, RectF dimensions, string text, float yFactor)
    {
        var pulse = (Math.Sin(_elapsedMilliseconds / 450d) + 1d) / 2d;
        canvas.Alpha = (float)(0.35d + pulse * 0.65d);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.018f, 16f, 28f);
        canvas.DrawString(text, dimensions.X + 40, dimensions.Y + dimensions.Height * yFactor,
            Math.Max(0, dimensions.Width - 80), 50, HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.Alpha = 1f;
    }

    private static void DrawChampionSelection(ICanvas canvas, RectF dimensions, Champion highlightedChampion)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.034f, 26f, 50f);
        canvas.DrawString("CHOOSE YOUR CHAMPION", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.12f, Math.Max(0, dimensions.Width - 80), 70,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.015f, 13f, 22f);
        canvas.DrawString("This choice will shape the world for the rest of the session.", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.21f, Math.Max(0, dimensions.Width - 80), 40,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        var cardTop = dimensions.Y + dimensions.Height * 0.32f;
        var cardHeight = dimensions.Height * 0.38f;
        var gap = Math.Max(18f, dimensions.Width * 0.018f);
        var totalWidth = dimensions.Width * 0.86f;
        var cardWidth = (totalWidth - gap * 2) / 3f;
        var startX = dimensions.X + (dimensions.Width - totalWidth) / 2f;

        DrawChampionCard(canvas, new RectF(startX, cardTop, cardWidth, cardHeight),
            "THE DRIVER", "Speed. Precision.", "Questionable braking.", "01",
            highlightedChampion == Champion.RacingDriver);
        DrawChampionCard(canvas, new RectF(startX + cardWidth + gap, cardTop, cardWidth, cardHeight),
            "THE CHARIOTEER", "Glory. Courage.", "Undefined behaviour.", "02",
            highlightedChampion == Champion.Charioteer);
        DrawChampionCard(canvas, new RectF(startX + (cardWidth + gap) * 2, cardTop, cardWidth, cardHeight),
            "THE SPACE PILOT", "Exploration. Thrusters.", "Naturally, Orbit.", "03",
            highlightedChampion == Champion.SpacePilot);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.014f, 13f, 22f);
        canvas.DrawString("LEFT STICK / A-D choose   •   A / ENTER / tap confirm", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.77f, Math.Max(0, dimensions.Width - 80), 40,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private static void DrawChampionCard(ICanvas canvas, RectF rect, string title, string line1, string line2, string number, bool isHighlighted)
    {
        canvas.FillColor = isHighlighted ? Color.FromArgb("#172033") : Color.FromArgb("#111827");
        canvas.FillRoundedRectangle(rect, 18);
        canvas.StrokeColor = isHighlighted ? Color.FromArgb("#F8D866") : Color.FromArgb("#475569");
        canvas.StrokeSize = isHighlighted ? 4 : 2;
        canvas.DrawRoundedRectangle(rect, 18);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#F8D866");
        canvas.FontSize = Math.Clamp(rect.Width * 0.15f, 26f, 46f);
        canvas.DrawString(number, rect.X, rect.Y + rect.Height * 0.10f, rect.Width, rect.Height * 0.24f,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(rect.Width * 0.072f, 14f, 23f);
        canvas.DrawString(title, rect.X + 10, rect.Y + rect.Height * 0.43f, rect.Width - 20, 34,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#CBD5E1");
        canvas.FontSize = Math.Clamp(rect.Width * 0.048f, 12f, 18f);
        canvas.DrawString(line1, rect.X + 10, rect.Y + rect.Height * 0.60f, rect.Width - 20, 28,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.DrawString(line2, rect.X + 10, rect.Y + rect.Height * 0.71f, rect.Width - 20, 28,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawChampionChosen(ICanvas canvas, RectF dimensions)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#F8D866");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.04f, 28f, 56f);
        canvas.DrawString(GetChampionName(_state.Champion), dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.35f, Math.Max(0, dimensions.Width - 80), 70,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.019f, 16f, 28f);
        canvas.DrawString(GetThemeTagline(_state.Champion), dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.48f, Math.Max(0, dimensions.Width - 80), 50,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        DrawContinuePrompt(canvas, dimensions, "A / ENTER  ·  REVEAL THE ORBIT CODE", 0.68f);
    }

    private static RectF GetCodePanel(RectF dimensions)
    {
        var margin = Math.Max(42f, dimensions.Width * 0.05f);
        var gap = Math.Max(28f, dimensions.Width * 0.035f);
        var contentTop = dimensions.Y + dimensions.Height * 0.16f;
        var contentHeight = dimensions.Height * 0.66f;
        var panelWidth = (dimensions.Width - margin * 2 - gap) / 2f;
        return new RectF(dimensions.X + margin, contentTop, panelWidth, contentHeight);
    }

    private static void DrawCodeRevealShell(ICanvas canvas, RectF dimensions)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.028f, 24f, 42f);
        canvas.DrawString("REAL ORBIT CODE → LIVE GAMEOBJECT", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.055f, Math.Max(0, dimensions.Width - 80), 60,
            HorizontalAlignment.Center, VerticalAlignment.Center);

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

        string[] lines =
        [
            "// OpeningScene",
            "Add(new ChampionGameObject(state));",
            "",
            "public sealed class ChampionGameObject",
            "    : GameObject",
            "{",
            "    public override void Render(",
            "        ICanvas canvas, RectF dimensions)",
            "    {",
            "        DrawChampion(canvas, dimensions,",
            "            _state.Champion);",
            "    }",
            "}"
        ];

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#E2E8F0");
        canvas.FontSize = Math.Clamp(rect.Width * 0.027f, 10f, 15f);
        var lineHeight = Math.Clamp(rect.Height * 0.044f, 18f, 25f);
        var y = rect.Y + rect.Height * 0.13f;
        foreach (var line in lines)
        {
            canvas.DrawString(line, rect.X + 24, y, rect.Width - 48, lineHeight,
                HorizontalAlignment.Left, VerticalAlignment.Center);
            y += lineHeight;
        }

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#F8D866");
        canvas.FontSize = Math.Clamp(rect.Width * 0.031f, 11f, 16f);
        canvas.DrawString($"Selected: {GetChampionName(_state.Champion)}", rect.X + 24,
            rect.Bottom - 40, rect.Width - 48, 24, HorizontalAlignment.Left, VerticalAlignment.Center);
    }

    private static string GetChampionName(Champion? champion) => champion switch
    {
        Champion.Charioteer => "THE CHARIOTEER",
        Champion.SpacePilot => "THE SPACE PILOT",
        _ => "THE RACING DRIVER"
    };

    private static string GetThemeTagline(Champion? champion) => champion switch
    {
        Champion.Charioteer => "The arena awaits.",
        Champion.SpacePilot => "Course plotted. Welcome to Orbit.",
        _ => "Engines ready. Lights out."
    };
}
