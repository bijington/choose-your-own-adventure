using ChooseYourOwnAdventure.Presentation;
using ChooseYourOwnAdventure.Live;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class RecapScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    private readonly LiveRaceState _raceState;

    public RecapScene(AdventureState state, LiveRaceState raceState)
    {
        _state = state;
        _raceState = raceState;
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "ADVENTURE COMPLETE",
            "One MAUI app. A game loop. An entire room playing together.");

        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        DrawSummary(canvas, new RectF(left.X, left.Y, left.Width, left.Height * .46f),
            "ORBIT", "Scenes · GameObjects\nUpdate + Render");
        DrawSummary(canvas, new RectF(left.X, left.Y + left.Height * .52f, left.Width, left.Height * .46f),
            "ORBIT.INPUT", "Controller · Keyboard\nPlayer intent");
        DrawSummary(canvas, new RectF(right.X, right.Y, right.Width, right.Height * .62f),
            "SIGNALR", "Audience input\nShared state\nReal-time multiplayer");

        var winner = _raceState.Snapshot.Winner;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.023f, 19f, 34f);
        canvas.DrawString(winner is null ? "THANK YOU" : $"RACE WINNER: {FormatWinner(winner.Value)}",
            right.X, right.Y + right.Height * .72f, right.Width, 96,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.016f, 14f, 23f);
        canvas.DrawString("Input affects state. Update evolves state. Render displays state.",
            left.X, dimensions.Bottom - 108, left.Width, 64,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.Recap, _state.Champion));
    }

    private static string FormatWinner(Champion champion) => champion switch
    {
        Champion.RacingDriver => "THE DRIVER",
        Champion.Charioteer => "THE CHARIOTEER",
        Champion.SpacePilot => "THE SPACE PILOT",
        _ => "THE CHAMPION"
    };

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
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(rect.Width * 0.052f, 14f, 21f);
        canvas.DrawString(body, rect.X + 20, rect.Y + 88, rect.Width - 40, rect.Height - 110,
            HorizontalAlignment.Center, VerticalAlignment.Top);
    }
}
