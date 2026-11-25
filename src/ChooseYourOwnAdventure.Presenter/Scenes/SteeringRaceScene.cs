using Adventure.Contracts;
using ChooseYourOwnAdventure.GameObjects;
using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class SteeringRaceScene(AdventureState state, AudienceVoteClient audience, QrCodeRenderer qr) : PresentationSceneBase
{
    private readonly Dictionary<int, Sprite> _sprites = new();
    private readonly Dictionary<int, double> _progress = new();
    private readonly Dictionary<int, double> _movingUntil = new();
    private double _elapsed;
    public override void Update(double millisecondsSinceLastUpdate)
    {
        base.Update(millisecondsSinceLastUpdate);
        _elapsed += millisecondsSinceLastUpdate;
        var snapshot = audience.Steering.Snapshot;
        foreach (var vehicle in snapshot.Vehicles)
        {
            if (snapshot.Phase == SteeringPhase.Racing && vehicle.Progress > _progress.GetValueOrDefault(vehicle.Champion) + 1e-7)
                _movingUntil[vehicle.Champion] = _elapsed + 120;
            _progress[vehicle.Champion] = vehicle.Progress;
            if (snapshot.Phase == SteeringPhase.Racing && _movingUntil.GetValueOrDefault(vehicle.Champion) > _elapsed)
            {
                if (!_sprites.TryGetValue(vehicle.Champion, out var sprite))
                    _sprites[vehicle.Champion] = sprite = ChampionSprites.CreateMotion((Champion)vehicle.Champion);
                sprite.Update(millisecondsSinceLastUpdate);
            }
        }
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        var snapshot = audience.Steering.Snapshot;
        DrawTitle(canvas, dimensions, "AFTER THE CREDITS…\nONE MORE LAP?",
            "Same rivals. Split power. Your team must now steer together.");
        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .016f, 15, 23);
        var message = state.IsRehearsing ? "PRACTICE · briefing only.\nSwitch to live audience to race."
            : snapshot.Phase switch
            {
                SteeringPhase.Inactive => "SOUTH / ENTER · prepare teams.\nThen check your phone for LEFT or RIGHT power.",
                SteeringPhase.Ready => "Everyone has a side.\nSOUTH / ENTER · start the bonus race.",
                SteeringPhase.Racing => "Tap together for speed.\nChange the balance to steer.\nOff course? You lose speed, not progress.",
                _ => $"{Name(snapshot.Winner)} wins the bonus lap!\nSOUTH / ENTER · return to the closing page."
            };
        canvas.DrawString(message, left.X, left.Y, left.Width, left.Height * .24f,
            HorizontalAlignment.Left, VerticalAlignment.Top);
        var rows = snapshot.Vehicles.Length > 0 ? snapshot.Vehicles : new[]
        {
            new SteeringVehicle(20, 0, 0, 0, 0, 0, 0, 0),
            new SteeringVehicle(50, 0, 0, 0, 0, 0, 0, 0),
            new SteeringVehicle(80, 0, 0, 0, 0, 0, 0, 0)
        };
        var rowHeight = left.Height * .22f;
        for (var i = 0; i < rows.Length; i++)
        {
            var vehicle = rows[i];
            var y = left.Y + left.Height * .28f + i * rowHeight;
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontColor = Accent;
            canvas.DrawString($"{Name(vehicle.Champion)} · {vehicle.Progress:P0}", left.X, y, left.Width, rowHeight * .24f,
                HorizontalAlignment.Left, VerticalAlignment.Center);
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;
            canvas.FontColor = Ink;
            canvas.FontSize = Math.Clamp(dimensions.Width * .012f, 12, 18);
            canvas.DrawString($"LEFT {vehicle.LeftPlayers} · RIGHT {vehicle.RightPlayers}    "
                + (Math.Abs(vehicle.Offset - vehicle.TargetOffset) > .5 ? "OFF COURSE" : "ON COURSE"),
                left.X, y + rowHeight * .25f, left.Width, rowHeight * .24f,
                HorizontalAlignment.Left, VerticalAlignment.Center);
            var gauge = new RectF(left.X, y + rowHeight * .58f, left.Width, 16);
            canvas.StrokeColor = Gold;
            canvas.StrokeSize = 2;
            canvas.DrawLine(gauge.X, gauge.Center.Y, gauge.Right, gauge.Center.Y);
            var targetX = gauge.Center.X + (float)vehicle.TargetOffset / 2 * gauge.Width / 2;
            canvas.FillColor = Color.FromArgb("#8D9A68");
            canvas.FillRectangle(targetX - gauge.Width * .125f, gauge.Y, gauge.Width * .25f, gauge.Height);
            canvas.FillColor = Accent;
            canvas.FillEllipse(gauge.Center.X + (float)vehicle.Offset / 2 * gauge.Width / 2 - 5, gauge.Y - 2, 10, 20);
        }
        // A wavy centreline makes the steering target visible throughout the lap.
        var track = new RectF(right.X + right.Width * .10f, right.Y + right.Height * .08f,
            right.Width * .80f, right.Height * .64f);
        var path = new PathF();
        for (var i = 0; i <= 160; i++)
        {
            var point = CoursePoint(track, i / 160d, .8 * Math.Sin(i / 160d * Math.PI * 4));
            if (i == 0) path.MoveTo(point); else path.LineTo(point);
        }
        canvas.StrokeColor = PresentationTheme.TrackSurface;
        canvas.StrokeSize = 40;
        canvas.DrawPath(path);
        canvas.StrokeColor = Gold;
        canvas.StrokeSize = 2;
        canvas.DrawPath(path);
        foreach (var vehicle in rows)
        {
            var point = CoursePoint(track, vehicle.Progress, vehicle.Offset);
            canvas.SaveState();
            canvas.Translate(point.X, point.Y);
            canvas.Rotate((float)(vehicle.Progress * 360));
            if (snapshot.Phase == SteeringPhase.Racing && _movingUntil.GetValueOrDefault(vehicle.Champion) > _elapsed
                && _sprites.TryGetValue(vehicle.Champion, out var sprite))
            {
                canvas.Rotate(90);
                sprite.Bounds = new RectF(-64, -80, 128, 160);
                sprite.Render(canvas, sprite.Bounds);
            }
            else ChampionSprites.DrawTopVehicle(canvas, (Champion)vehicle.Champion);
            canvas.RestoreState();
        }
        var size = Math.Min(95, right.Width * .23f);
        qr.Draw(canvas, new RectF(right.Right - size, right.Bottom - size, size, size));
        canvas.FontColor = Muted;
        canvas.FontSize = 12;
        canvas.DrawString("Keep the original browser open.\nYour original champion is still your team.\nOne-person teams get both controls.",
            right.X, right.Y + right.Height * .78f, right.Width - size - 12, right.Height * .22f,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        DrawPrompt(canvas, dimensions, "SOUTH / ENTER · PREPARE / START / FINISH    EAST / ESC · CLOSE BONUS RACE");
        DrawPageNumber(canvas, dimensions, 405);
    }
    private static PointF CoursePoint(RectF track, double progress, double offset)
    {
        var angle = progress * Math.PI * 2 - Math.PI / 2;
        return new PointF(track.Center.X + (float)Math.Cos(angle) * (track.Width / 2 + (float)offset * 26),
            track.Center.Y + (float)Math.Sin(angle) * (track.Height / 2 + (float)offset * 26));
    }
    private static string Name(int? champion) => champion switch
    { 20 => "DRIVER", 50 => "CHARIOTEER", 80 => "PILOT", _ => "A champion" };
}
