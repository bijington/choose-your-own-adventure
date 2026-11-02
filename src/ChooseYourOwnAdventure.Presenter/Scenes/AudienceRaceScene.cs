using ChooseYourOwnAdventure.GameObjects;
using ChooseYourOwnAdventure.Live;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class AudienceRaceScene : GameScene
{
    private readonly LiveRaceState _raceState;
    private readonly QrCodeRenderer _qrCodeRenderer;
    private readonly LiveAudienceOptions _options;
    private readonly AudienceRaceVehicle _driver;
    private readonly AudienceRaceVehicle _charioteer;
    private readonly AudienceRaceVehicle _pilot;

    public AudienceRaceScene(
        LiveRaceState raceState,
        QrCodeRenderer qrCodeRenderer,
        LiveAudienceOptions options)
    {
        _raceState = raceState;
        _qrCodeRenderer = qrCodeRenderer;
        _options = options;

        _driver = new AudienceRaceVehicle(Champion.RacingDriver, raceState, -26f);
        _charioteer = new AudienceRaceVehicle(Champion.Charioteer, raceState, 0f);
        _pilot = new AudienceRaceVehicle(Champion.SpacePilot, raceState, 26f);

        Add(_driver);
        Add(_charioteer);
        Add(_pilot);
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawBackground(canvas, dimensions);
        DrawHeading(canvas, dimensions);
        DrawTrack(canvas, dimensions);
        DrawStandings(canvas, dimensions);
        DrawJoinPrompt(canvas, dimensions);

        base.Render(canvas, dimensions);
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
        var snapshot = _raceState.Snapshot;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.034f, 26f, 50f);
        canvas.DrawString("THE AUDIENCE GRAND PRIX", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.045f, dimensions.Width - 80, 56,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.FontSize = Math.Clamp(dimensions.Width * 0.015f, 13f, 21f);
        canvas.FontColor = snapshot.Phase switch
        {
            RacePhase.Racing => Color.FromArgb("#F8D866"),
            RacePhase.Finished => Color.FromArgb("#86EFAC"),
            _ => Color.FromArgb("#94A3B8")
        };

        var subtitle = snapshot.Phase switch
        {
            RacePhase.Racing => "Every audience tap moves the champion they voted for. TAP! TAP! TAP!",
            RacePhase.Finished when snapshot.Winner is { } winner => $"{DisplayName(winner)} WINS!",
            _ => "A / ENTER starts the race · B / ESC resets · teams are remembered from the opening vote"
        };

        canvas.DrawString(subtitle, dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.11f, dimensions.Width - 80, 34,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private static void DrawTrack(ICanvas canvas, RectF dimensions)
    {
        var track = AudienceRaceLayout.GetTrackBounds(dimensions);

        canvas.StrokeColor = Color.FromArgb("#1E293B");
        canvas.StrokeSize = 92;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);

        canvas.StrokeColor = Color.FromArgb("#475569");
        canvas.StrokeSize = 2;
        canvas.DrawEllipse(track.X - 26, track.Y - 16, track.Width + 52, track.Height + 32);
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
        canvas.DrawEllipse(track.X + 26, track.Y + 16,
            Math.Max(1, track.Width - 52), Math.Max(1, track.Height - 32));

        // Start / finish line at the top of the course.
        var centreX = track.X + track.Width / 2f;
        var topY = track.Y - 48f;
        const float cell = 9f;
        for (var row = 0; row < 4; row++)
        {
            for (var col = 0; col < 8; col++)
            {
                canvas.FillColor = (row + col) % 2 == 0 ? Colors.White : Color.FromArgb("#111827");
                canvas.FillRectangle(centreX - 36 + col * cell, topY + row * cell, cell, cell);
            }
        }
    }

    private void DrawStandings(ICanvas canvas, RectF dimensions)
    {
        var snapshot = _raceState.Snapshot;
        var panelWidth = Math.Min(330f, dimensions.Width * 0.28f);
        var panel = new RectF(dimensions.X + 42, dimensions.Y + dimensions.Height * 0.22f,
            panelWidth, dimensions.Height * 0.47f);

        canvas.FillColor = Color.FromArgb("#0F172A");
        canvas.FillRoundedRectangle(panel, 18);
        canvas.StrokeColor = Color.FromArgb("#334155");
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(panel, 18);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(panel.Width * 0.07f, 16f, 22f);
        canvas.DrawString("LIVE RACE", panel.X + 18, panel.Y + 16, panel.Width - 36, 28,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        DrawTeamRow(canvas, panel, 62, "🏎 DRIVER", snapshot.RacingDriverProgress, snapshot.RacingDriverTeamSize,
            snapshot.Winner == Champion.RacingDriver);
        DrawTeamRow(canvas, panel, 132, "🛡 CHARIOTEER", snapshot.CharioteerProgress, snapshot.CharioteerTeamSize,
            snapshot.Winner == Champion.Charioteer);
        DrawTeamRow(canvas, panel, 202, "🚀 PILOT", snapshot.SpacePilotProgress, snapshot.SpacePilotTeamSize,
            snapshot.Winner == Champion.SpacePilot);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#64748B");
        canvas.FontSize = 11;
        canvas.DrawString("Tap strength is normalised by team size, so a smaller team can still win.",
            panel.X + 18, panel.Bottom - 58, panel.Width - 36, 44,
            HorizontalAlignment.Left, VerticalAlignment.Center);
    }

    private static void DrawTeamRow(ICanvas canvas, RectF panel, float offsetY, string name,
        double progress, int teamSize, bool winner)
    {
        var y = panel.Y + offsetY;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = winner ? Color.FromArgb("#F8D866") : Colors.White;
        canvas.FontSize = 14;
        canvas.DrawString(name, panel.X + 18, y, panel.Width - 100, 22,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = 11;
        canvas.DrawString($"{teamSize} racer{(teamSize == 1 ? "" : "s")}", panel.Right - 92, y,
            74, 22, HorizontalAlignment.Right, VerticalAlignment.Center);

        var bar = new RectF(panel.X + 18, y + 29, panel.Width - 36, 15);
        canvas.FillColor = Color.FromArgb("#1E293B");
        canvas.FillRoundedRectangle(bar, 7);
        canvas.FillColor = winner ? Color.FromArgb("#F8D866") : Color.FromArgb("#38BDF8");
        canvas.FillRoundedRectangle(new RectF(bar.X, bar.Y, bar.Width * (float)Math.Clamp(progress, 0d, 1d), bar.Height), 7);
    }

    private void DrawJoinPrompt(ICanvas canvas, RectF dimensions)
    {
        var size = Math.Clamp(dimensions.Width * 0.10f, 82f, 130f);
        var qr = new RectF(dimensions.Right - size - 58, dimensions.Bottom - size - 68, size, size);
        _qrCodeRenderer.Draw(canvas, qr);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#CBD5E1");
        canvas.FontSize = 12;
        canvas.DrawString("CLOSED THE PAGE? SCAN TO REJOIN YOUR TEAM", qr.X - 45, qr.Y - 28, qr.Width + 90, 22,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#64748B");
        canvas.FontSize = 10;
        canvas.DrawString(_options.JoinUrl, qr.X - 90, qr.Bottom + 8, qr.Width + 180, 20,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private static string DisplayName(Champion champion) => champion switch
    {
        Champion.RacingDriver => "THE DRIVER",
        Champion.Charioteer => "THE CHARIOTEER",
        Champion.SpacePilot => "THE SPACE PILOT",
        _ => "THE CHAMPION"
    };
}
