using ChooseYourOwnAdventure.GameObjects;
using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class AudienceRaceScene : GameScene
{
    private readonly AdventureState _state;
    private double _animationTime;
    private readonly Dictionary<Champion, (double Progress, double JumpUntil)> _launches = new();

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

    public override void Update(double millisecondsSinceLastUpdate)
    {
        base.Update(millisecondsSinceLastUpdate);
        _animationTime += millisecondsSinceLastUpdate;
        var snapshot = _raceState.Snapshot;
        if (snapshot.Phase == RacePhase.Ready)
        {
            _launches.Clear();
            return;
        }
        foreach (var champion in Enum.GetValues<Champion>())
        {
            var progress = _raceState.ProgressFor(champion);
            var previous = _launches.GetValueOrDefault(champion);
            var jumped = snapshot.Mode == BoostMode.ChargeAndRelease && progress - previous.Progress >= .249d;
            _launches[champion] = (progress, jumped ? _animationTime + 750 : previous.JumpUntil);
        }
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawBackground(canvas, dimensions);
        DrawHeading(canvas, dimensions);
        DrawTrack(canvas, dimensions);
        DrawChallenge(canvas, dimensions);
        DrawChargeMeters(canvas, dimensions);
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

        var subtitle = _state.IsRehearsing
            ? "PRACTICE MODE · Tap the right page to skip the race, or the left page to go back."
            : snapshot.Phase switch
        {
            RacePhase.Racing => snapshot.Mode == BoostMode.ChargeAndRelease
                ? "ANCIENT LAUNCH PADS · Charge together. At 100%, your team jumps!"
                : "THE WINDING ROAD · Every tap moves your original champion!",
            RacePhase.Finished when snapshot.Winner is { } winner => $"{DisplayName(winner)} WINS!  ·  SOUTH / ENTER FOR THE PODIUM",
            _ => "SOUTH / ENTER starts the race · EAST / ESC resets · teams are remembered from the opening vote"
        };

        canvas.DrawString(subtitle, dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.11f, dimensions.Width - 80, 34,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawTrack(ICanvas canvas, RectF dimensions)
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

        if (_raceState.Snapshot.Mode == BoostMode.ChargeAndRelease)
        {
            // Four ancient pads mark the quarter-lap jumps around the course.
            for (var pad = 0; pad < 4; pad++)
            {
                var angle = pad * Math.PI / 2 - Math.PI / 2;
                var x = track.Center.X + (float)Math.Cos(angle) * track.Width / 2;
                var y = track.Center.Y + (float)Math.Sin(angle) * track.Height / 2;
                canvas.SaveState();
                canvas.Alpha = .22f;
                canvas.FillColor = Color.FromArgb("#DCA530");
                canvas.FillEllipse(x - 32, y - 32, 64, 64);
                canvas.Alpha = 1;
                canvas.StrokeColor = PresentationTheme.AccentGold;
                canvas.StrokeSize = 3;
                canvas.DrawEllipse(x - 22, y - 22, 44, 44);
                var rune = new PathF();
                rune.MoveTo(x, y - 14);
                rune.LineTo(x + 11, y);
                rune.LineTo(x, y + 14);
                rune.LineTo(x - 11, y);
                rune.Close();
                canvas.StrokeColor = PresentationTheme.AccentRed;
                canvas.DrawPath(rune);
                canvas.RestoreState();
            }
        }
        else
        {
            canvas.SaveState();
            canvas.StrokeColor = PresentationTheme.ParchmentLight;
            canvas.StrokeSize = 3;
            canvas.StrokeDashPattern = new[] { 10f, 8f };
            canvas.DrawEllipse(track);
            canvas.RestoreState();
        }

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

    private void DrawChallenge(ICanvas canvas, RectF dimensions)
    {
        var track = AudienceRaceLayout.GetTrackBounds(dimensions);
        var phase = _raceState.Snapshot.Phase;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.AccentRed;
        canvas.FontSize = Math.Clamp(dimensions.Width * .019f, 18f, 28f);
        canvas.DrawString(phase == RacePhase.Finished ? "THE ROOM WROTE THE ENDING" : "THE FINAL CHALLENGE",
            track.X + track.Width * .16f, track.Center.Y - 44, track.Width * .68f, 40,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .014f, 14f, 21f);
        canvas.DrawString(phase switch
        {
            RacePhase.Ready => "A race… or a headcount?\nWhat if one team has twice as many people?",
            RacePhase.Racing => "Power your champion through the last stretch!",
            _ => "Your choices. Your teams. Your finish."
        }, track.X + track.Width * .18f, track.Center.Y, track.Width * .64f, 68,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawChargeMeters(ICanvas canvas, RectF dimensions)
    {
        var snapshot = _raceState.Snapshot;
        if (snapshot.Mode != BoostMode.ChargeAndRelease) return;
        var track = AudienceRaceLayout.GetTrackBounds(dimensions);
        var x = track.X + track.Width * .25f;
        var width = track.Width * .5f;
        var y = track.Center.Y + 80;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.AccentRed;
        canvas.FontSize = 14;
        canvas.DrawString("LAUNCH ENERGY · AUTOMATIC JUMP AT 100%", x, y - 26, width, 24,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        var rows = new[] { (Champion.RacingDriver, "DRIVER", snapshot.RacingDriverCharge),
            (Champion.Charioteer, "CHARIOT", snapshot.CharioteerCharge),
            (Champion.SpacePilot, "PILOT", snapshot.SpacePilotCharge) };
        foreach (var (champion, label, charge) in rows)
        {
            canvas.FontColor = PresentationTheme.Ink;
            canvas.DrawString(label, x, y, 86, 24, HorizontalAlignment.Left, VerticalAlignment.Center);
            var meter = new RectF(x + 90, y + 6, Math.Max(10, width - 140), 12);
            canvas.FillColor = PresentationTheme.TrackSurface;
            canvas.FillRoundedRectangle(meter, 6);
            canvas.FillColor = PresentationTheme.AccentRed;
            canvas.FillRoundedRectangle(new RectF(meter.X, meter.Y, meter.Width * (float)Math.Clamp(charge, 0, 1), meter.Height), 6);
            var feedback = _launches.GetValueOrDefault(champion).JumpUntil > _animationTime ? "JUMP!" : $"{charge:P0}";
            canvas.DrawString(feedback, meter.Right + 5, y, 45, 24, HorizontalAlignment.Left, VerticalAlignment.Center);
            y += 28;
        }
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
