using ChooseYourOwnAdventure.Live;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class RecapScene : PresentationSceneBase
{
    private readonly LiveRaceState _raceState;

    public RecapScene(LiveRaceState raceState)
    {
        _raceState = raceState;
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "ADVENTURE COMPLETE",
            "One MAUI app. A game loop. An entire room playing together.");

        var margin = dimensions.Width * 0.07f;
        var gap = dimensions.Width * 0.025f;
        var width = (dimensions.Width - margin * 2 - gap * 2) / 3f;
        var top = dimensions.Y + dimensions.Height * 0.28f;
        var height = dimensions.Height * 0.38f;

        DrawSummary(canvas, new RectF(dimensions.X + margin, top, width, height),
            "ORBIT", "Scenes\nGameObjects\nUpdate + Render");
        DrawSummary(canvas, new RectF(dimensions.X + margin + width + gap, top, width, height),
            "ORBIT.INPUT", "Controller\nKeyboard\nPlayer intent");
        DrawSummary(canvas, new RectF(dimensions.X + margin + (width + gap) * 2, top, width, height),
            "SIGNALR", "Audience input\nShared state\nReal-time multiplayer");

        var winner = _raceState.Snapshot.Winner;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.023f, 19f, 34f);
        canvas.DrawString(winner is null ? "THANK YOU" : $"RACE WINNER: {winner}",
            dimensions.X + 60, dimensions.Y + dimensions.Height * 0.73f, dimensions.Width - 120, 48,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.016f, 14f, 23f);
        canvas.DrawString("Input affects state. Update evolves state. Render displays state.",
            dimensions.X + 60, dimensions.Y + dimensions.Height * 0.82f, dimensions.Width - 120, 38,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private static void DrawSummary(ICanvas canvas, RectF rect, string title, string body)
    {
        canvas.FillColor = Panel;
        canvas.FillRoundedRectangle(rect, 18);
        canvas.StrokeColor = Border;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 18);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(rect.Width * 0.065f, 17f, 26f);
        canvas.DrawString(title, rect.X + 18, rect.Y + 24, rect.Width - 36, 36,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#E2E8F0");
        canvas.FontSize = Math.Clamp(rect.Width * 0.052f, 14f, 21f);
        canvas.DrawString(body, rect.X + 20, rect.Y + 88, rect.Width - 40, rect.Height - 110,
            HorizontalAlignment.Center, VerticalAlignment.Top);
    }
}
