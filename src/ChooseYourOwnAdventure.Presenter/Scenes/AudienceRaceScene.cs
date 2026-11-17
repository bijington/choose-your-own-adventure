using ChooseYourOwnAdventure.GameObjects;
using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class AudienceRaceScene : GameScene
{
    private readonly AdventureState _state;

    private readonly LiveRaceState _raceState;
    private readonly QrCodeRenderer _qrCodeRenderer;
    private readonly LiveAudienceOptions _options;
    private readonly AudienceRaceVehicle _driver;
    private readonly AudienceRaceVehicle _charioteer;
    private readonly AudienceRaceVehicle _pilot;

    public AudienceRaceScene(
        AdventureState state,
        LiveRaceState raceState,
        QrCodeRenderer qrCodeRenderer,
        LiveAudienceOptions options)
    {
        _state = state;
        _raceState = raceState;
        _qrCodeRenderer = qrCodeRenderer;
        _options = options;

        _driver = new AudienceRaceVehicle(Champion.RacingDriver, raceState, -34f);
        _charioteer = new AudienceRaceVehicle(Champion.Charioteer, raceState, 0f);
        _pilot = new AudienceRaceVehicle(Champion.SpacePilot, raceState, 34f);

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
        PresentationTheme.DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.AudienceRace, _state.Champion));
    }

    private static void DrawBackground(ICanvas canvas, RectF dimensions)
    {
        PresentationTheme.DrawParchment(canvas, dimensions);
    }

    private void DrawHeading(ICanvas canvas, RectF dimensions)
    {
        var snapshot = _raceState.Snapshot;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.034f, 26f, 50f);
        canvas.DrawString("THE AUDIENCE GRAND PRIX", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.025f, dimensions.Width - 80, 86,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.FontSize = Math.Clamp(dimensions.Width * 0.015f, 13f, 21f);
        canvas.FontColor = snapshot.Phase switch
        {
            RacePhase.Racing => PresentationTheme.AccentRed,
            RacePhase.Finished => PresentationTheme.AccentRed,
            _ => PresentationTheme.MutedInk
        };

        var subtitle = snapshot.Phase switch
        {
            RacePhase.Racing => "Every audience tap moves the champion they voted for. TAP! TAP! TAP!",
            RacePhase.Finished when snapshot.Winner is { } winner => $"{DisplayName(winner)} WINS!  ·  SOUTH / ENTER FOR THE PODIUM",
            _ => "SOUTH / ENTER starts the race · EAST / ESC resets · teams are remembered from the opening vote"
        };

        canvas.DrawString(subtitle, dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.11f, dimensions.Width - 80, 34,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private static void DrawTrack(ICanvas canvas, RectF dimensions)
    {
        var track = AudienceRaceLayout.GetTrackBounds(dimensions);

        canvas.StrokeColor = PresentationTheme.TrackSurface;
        canvas.StrokeSize = 112;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);

        canvas.StrokeColor = PresentationTheme.TrackEdge;
        canvas.StrokeSize = 2;
        canvas.DrawEllipse(track.X - 26, track.Y - 16, track.Width + 52, track.Height + 32);
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
        canvas.DrawEllipse(track.X + 26, track.Y + 16,
            Math.Max(1, track.Width - 52), Math.Max(1, track.Height - 32));

        // A proper finish line runs across the full thickness of the course.
        var centreX = track.X + track.Width / 2f;
        var centreY = track.Y + track.Height / 2f;
        var outerTop = centreY - track.Height / 2f - 56f;
        const float cell = 8f;
        const int columns = 5;
        const int rows = 14;
        var stripWidth = columns * cell;
        var stripX = centreX - stripWidth / 2f;
        var stripY = outerTop;

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < columns; col++)
            {
                canvas.FillColor = (row + col) % 2 == 0 ? PresentationTheme.ParchmentLight : PresentationTheme.Ink;
                canvas.FillRectangle(stripX + col * cell, stripY + row * cell, cell, cell);
            }
        }

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.AccentRed;
        canvas.FontSize = 12;
        canvas.DrawString("FINISH", centreX - 48, stripY - 26, 96, 20,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawStandings(ICanvas canvas, RectF dimensions)
    {
        var snapshot = _raceState.Snapshot;
        var panelWidth = Math.Min(330f, dimensions.Width * 0.28f);
        var panel = new RectF(dimensions.X + 42, dimensions.Y + dimensions.Height * 0.22f,
            panelWidth, dimensions.Height * 0.47f);

        canvas.FillColor = PresentationTheme.CodePanel;
        canvas.FillRoundedRectangle(panel, 18);
        canvas.StrokeColor = PresentationTheme.CodePanelBorder;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(panel, 18);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.Ink;
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
        canvas.FontColor = PresentationTheme.MutedInk;
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
        canvas.FontColor = winner ? PresentationTheme.AccentRed : PresentationTheme.Ink;
        canvas.FontSize = 14;
        canvas.DrawString(name, panel.X + 18, y, panel.Width - 100, 22,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.MutedInk;
        canvas.FontSize = 11;
        canvas.DrawString($"{teamSize} racer{(teamSize == 1 ? "" : "s")}", panel.Right - 92, y,
            74, 22, HorizontalAlignment.Right, VerticalAlignment.Center);

        var bar = new RectF(panel.X + 18, y + 29, panel.Width - 36, 15);
        canvas.FillColor = Color.FromArgb("#E3CFA4");
        canvas.FillRoundedRectangle(bar, 7);
        canvas.FillColor = winner ? PresentationTheme.AccentRed : PresentationTheme.AccentGold;
        canvas.FillRoundedRectangle(new RectF(bar.X, bar.Y, bar.Width * (float)Math.Clamp(progress, 0d, 1d), bar.Height), 7);
    }

    private void DrawJoinPrompt(ICanvas canvas, RectF dimensions)
    {
        var size = Math.Clamp(dimensions.Width * 0.10f, 82f, 130f);
        var qr = new RectF(dimensions.Right - size - 58, dimensions.Bottom - size - 68, size, size);
        _qrCodeRenderer.Draw(canvas, qr);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = 12;
        canvas.DrawString("CLOSED THE PAGE? SCAN TO REJOIN YOUR TEAM", qr.X - 45, qr.Y - 28, qr.Width + 90, 22,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.MutedInk;
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
