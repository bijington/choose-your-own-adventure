using ChooseYourOwnAdventure.Live;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.GameObjects;

public sealed class AudienceRaceVehicle : GameObject
{
    private readonly Champion _champion;
    private readonly LiveRaceState _raceState;
    private readonly float _laneOffset;
    private double _displayProgress;

    public AudienceRaceVehicle(Champion champion, LiveRaceState raceState, float laneOffset)
    {
        _champion = champion;
        _raceState = raceState;
        _laneOffset = laneOffset;
    }

    public Champion Champion => _champion;
    public double DisplayProgress => _displayProgress;

    public override void Update(double millisecondsSinceLastUpdate)
    {
        base.Update(millisecondsSinceLastUpdate);

        var target = _raceState.ProgressFor(_champion);
        var elapsed = millisecondsSinceLastUpdate / 1000d;
        var smoothing = Math.Clamp(elapsed * 7d, 0d, 1d);
        _displayProgress += (target - _displayProgress) * smoothing;

        if (Math.Abs(target - _displayProgress) < 0.0002d)
        {
            _displayProgress = target;
        }
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        base.Render(canvas, dimensions);

        var track = AudienceRaceLayout.GetTrackBounds(dimensions);
        var angle = _displayProgress * Math.PI * 2d - Math.PI / 2d;
        var rx = track.Width * 0.5f + _laneOffset;
        var ry = track.Height * 0.5f + _laneOffset * 0.6f;
        var centreX = track.X + track.Width / 2f;
        var centreY = track.Y + track.Height / 2f;

        var cos = (float)Math.Cos(angle);
        var sin = (float)Math.Sin(angle);
        var x = centreX + cos * rx;
        var y = centreY + sin * ry;
        var dx = -rx * sin;
        var dy = ry * cos;
        var heading = (float)(Math.Atan2(dy, dx) * 180d / Math.PI);

        canvas.SaveState();
        canvas.Translate(x, y);
        canvas.Rotate(heading);

        switch (_champion)
        {
            case Champion.RacingDriver:
                DrawCar(canvas);
                break;
            case Champion.Charioteer:
                DrawChariot(canvas);
                break;
            case Champion.SpacePilot:
                DrawShip(canvas);
                break;
        }

        canvas.RestoreState();
    }

    private static void DrawCar(ICanvas canvas)
    {
        canvas.FillColor = Color.FromArgb("#0F172A");
        canvas.FillRectangle(-17, -13, 8, 6);
        canvas.FillRectangle(-17, 7, 8, 6);
        canvas.FillRectangle(9, -13, 8, 6);
        canvas.FillRectangle(9, 7, 8, 6);
        canvas.FillColor = Color.FromArgb("#EF4444");
        canvas.FillRoundedRectangle(new RectF(-22, -9, 42, 18), 6);
        canvas.FillColor = Colors.White;
        canvas.FillRoundedRectangle(new RectF(-5, -6, 14, 12), 4);
    }

    private static void DrawChariot(ICanvas canvas)
    {
        canvas.FillColor = Color.FromArgb("#78350F");
        canvas.FillRoundedRectangle(new RectF(-11, -14, 24, 28), 5);
        canvas.StrokeColor = Color.FromArgb("#F8D866");
        canvas.StrokeSize = 3;
        canvas.DrawEllipse(-4, -20, 12, 12);
        canvas.DrawEllipse(-4, 8, 12, 12);
        canvas.FillColor = Color.FromArgb("#D97706");
        canvas.FillEllipse(16, -12, 14, 10);
        canvas.FillEllipse(16, 2, 14, 10);
    }

    private static void DrawShip(ICanvas canvas)
    {
        var path = new PathF();
        path.MoveTo(24, 0);
        path.LineTo(-16, -15);
        path.LineTo(-7, 0);
        path.LineTo(-16, 15);
        path.Close();
        canvas.FillColor = Color.FromArgb("#818CF8");
        canvas.FillPath(path);
        canvas.FillColor = Colors.White;
        canvas.FillEllipse(-2, -5, 10, 10);
        canvas.FillColor = Color.FromArgb("#F8D866");
        canvas.FillRoundedRectangle(new RectF(-25, -5, 12, 10), 4);
    }
}

public static class AudienceRaceLayout
{
    public static RectF GetTrackBounds(RectF dimensions)
    {
        var width = dimensions.Width * 0.67f;
        var height = dimensions.Height * 0.52f;
        return new RectF(
            dimensions.X + (dimensions.Width - width) / 2f,
            dimensions.Y + dimensions.Height * 0.22f,
            width,
            height);
    }
}
