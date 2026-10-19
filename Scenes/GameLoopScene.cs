using ChooseYourOwnAdventure.GameObjects;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class GameLoopScene : GameScene
{
    private readonly AdventureState _state;
    private readonly LoopRacer _racer;

    public GameLoopScene(AdventureState state)
    {
        _state = state;
        _state.CurrentLocation = "The Looping Speedway";
        _racer = new LoopRacer(state);
        Add(_racer);
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawBackground(canvas, dimensions);
        DrawHeading(canvas, dimensions);
        DrawTrack(canvas, dimensions);
        DrawStations(canvas, dimensions);

        if (_state.GameLoopStage == GameLoopStage.FirmwareComparison)
        {
            DrawFirmwareComparison(canvas, dimensions);
        }
        else if (_state.GameLoopStage == GameLoopStage.Running)
        {
            DrawRunningExplanation(canvas, dimensions);
        }
        else
        {
            DrawOverview(canvas, dimensions);
        }

        base.Render(canvas, dimensions);
    }

    public static RectF GetTrackBounds(RectF dimensions)
    {
        var width = dimensions.Width * 0.62f;
        var height = dimensions.Height * 0.48f;
        return new RectF(
            dimensions.X + (dimensions.Width - width) / 2f,
            dimensions.Y + dimensions.Height * 0.20f,
            width,
            height);
    }

    private static void DrawBackground(ICanvas canvas, RectF dimensions)
    {
        canvas.FillColor = Color.FromArgb("#07111F");
        canvas.FillRectangle(dimensions);
        canvas.StrokeColor = Color.FromArgb("#26364D");
        canvas.StrokeSize = 2;
        canvas.DrawRectangle(dimensions.X + 24, dimensions.Y + 24,
            Math.Max(0, dimensions.Width - 48), Math.Max(0, dimensions.Height - 48));
    }

    private void DrawHeading(ICanvas canvas, RectF dimensions)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.031f, 24f, 46f);
        canvas.DrawString(GetTitle(), dimensions.X + 40, dimensions.Y + dimensions.Height * 0.055f,
            Math.Max(0, dimensions.Width - 80), 52, HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.014f, 13f, 21f);
        canvas.DrawString("A game keeps moving even when nobody clicks a button.", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.115f, Math.Max(0, dimensions.Width - 80), 36,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawTrack(ICanvas canvas, RectF dimensions)
    {
        var track = GetTrackBounds(dimensions);
        canvas.StrokeColor = GetTrackColor();
        canvas.StrokeSize = 28;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
        canvas.StrokeColor = Color.FromArgb("#0F172A");
        canvas.StrokeSize = 3;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
    }

    private static void DrawStations(ICanvas canvas, RectF dimensions)
    {
        var track = GetTrackBounds(dimensions);
        var stations = new[]
        {
            ("INPUT", (track.X + track.Width / 2f), track.Y),
            ("UPDATE", track.Right, (track.Y + track.Height / 2f)),
            ("RENDER", (track.X + track.Width / 2f), track.Bottom),
            ("WAIT", track.X, (track.Y + track.Height / 2f))
        };

        foreach (var (label, x, y) in stations)
        {
            canvas.FillColor = Color.FromArgb("#111827");
            canvas.FillEllipse(x - 27, y - 27, 54, 54);
            canvas.StrokeColor = Color.FromArgb("#F8D866");
            canvas.StrokeSize = 3;
            canvas.DrawEllipse(x - 27, y - 27, 54, 54);

            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontColor = Colors.White;
            canvas.FontSize = 13;
            canvas.DrawString(label, x - 50, y - 11, 100, 22,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }

    private static void DrawOverview(ICanvas canvas, RectF dimensions)
    {
        DrawInfoPanel(canvas, dimensions,
            "BUSINESS APP",
            "event → command → state change → UI update → wait",
            "GAME",
            "input → update → render → wait → repeat");
    }

    private static void DrawFirmwareComparison(ICanvas canvas, RectF dimensions)
    {
        DrawInfoPanel(canvas, dimensions,
            "FIRMWARE SUPERLOOP",
            "while (true) { ReadInputs(); UpdateState(); WriteOutputs(); Delay(); }",
            "GAME LOOP",
            "while (running) { Input(); Update(); Render(); Wait(); }");
    }

    private static void DrawRunningExplanation(ICanvas canvas, RectF dimensions)
    {
        DrawInfoPanel(canvas, dimensions,
            "ORBIT UPDATE",
            "_progress += elapsed * speed;",
            "ORBIT RENDER",
            "draw the current world state");
    }

    private static void DrawInfoPanel(ICanvas canvas, RectF dimensions,
        string leftTitle, string leftBody, string rightTitle, string rightBody)
    {
        var top = dimensions.Y + dimensions.Height * 0.74f;
        var width = dimensions.Width * 0.40f;
        var height = dimensions.Height * 0.16f;
        var gap = dimensions.Width * 0.04f;
        var left = new RectF((dimensions.X + dimensions.Width / 2f) - gap / 2f - width, top, width, height);
        var right = new RectF((dimensions.X + dimensions.Width / 2f) + gap / 2f, top, width, height);
        DrawInfoCard(canvas, left, leftTitle, leftBody);
        DrawInfoCard(canvas, right, rightTitle, rightBody);
    }

    private static void DrawInfoCard(ICanvas canvas, RectF rect, string title, string body)
    {
        canvas.FillColor = Color.FromArgb("#0F172A");
        canvas.FillRoundedRectangle(rect, 14);
        canvas.StrokeColor = Color.FromArgb("#334155");
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 14);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#F8D866");
        canvas.FontSize = Math.Clamp(rect.Width * 0.036f, 12f, 18f);
        canvas.DrawString(title, rect.X + 16, rect.Y + 12, rect.Width - 32, 24,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#E2E8F0");
        canvas.FontSize = Math.Clamp(rect.Width * 0.031f, 11f, 16f);
        canvas.DrawString(body, rect.X + 16, rect.Y + 40, rect.Width - 32, rect.Height - 52,
            HorizontalAlignment.Left, VerticalAlignment.Top);
    }

    private string GetTitle() => _state.Champion switch
    {
        Champion.Charioteer => "THE LOOPING ARENA",
        Champion.SpacePilot => "THE ORBITAL LOOP",
        _ => "THE LOOPING SPEEDWAY"
    };

    private Color GetTrackColor() => _state.Champion switch
    {
        Champion.Charioteer => Color.FromArgb("#B45309"),
        Champion.SpacePilot => Color.FromArgb("#4338CA"),
        _ => Color.FromArgb("#475569")
    };
}
