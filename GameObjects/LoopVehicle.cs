using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.GameObjects;

public enum LoopStation
{
    Input = 0,
    Update = 1,
    Render = 2,
    Wait = 3
}

/// <summary>
/// The vehicle that demonstrates the game loop by literally travelling around it.
/// Movement happens in Orbit's Update method and the current state is drawn in Render.
/// </summary>
public sealed class LoopVehicle : GameObject
{
    private const double TravelDurationMilliseconds = 1150d;
    private const double StationPauseMilliseconds = 700d;

    private readonly AdventureState _state;
    private int _stationIndex;
    private double _phaseElapsed;
    private double _pulseElapsed;
    private bool _isPaused = true;
    private double _progress;

    public LoopVehicle(AdventureState state)
    {
        _state = state;
    }

    public double Progress => _progress;

    public LoopStation CurrentStation => (LoopStation)_stationIndex;

    public bool IsPausedAtStation => _isPaused;

    public float StationPulse => (float)(0.5d + (Math.Sin(_pulseElapsed * 0.008d) + 1d) * 0.25d);

    public override void Update(double millisecondsSinceLastUpdate)
    {
        base.Update(millisecondsSinceLastUpdate);

        _phaseElapsed += millisecondsSinceLastUpdate;
        _pulseElapsed += millisecondsSinceLastUpdate;

        if (_isPaused)
        {
            if (_phaseElapsed >= StationPauseMilliseconds)
            {
                _phaseElapsed = 0d;
                _isPaused = false;
            }

            return;
        }

        var segmentProgress = Math.Clamp(_phaseElapsed / TravelDurationMilliseconds, 0d, 1d);
        var easedProgress = EaseInOut(segmentProgress);

        // Each station is exactly one quarter of the loop apart.
        _progress = ((_stationIndex * 0.25d) + (easedProgress * 0.25d)) % 1d;

        if (segmentProgress < 1d)
        {
            return;
        }

        _stationIndex = (_stationIndex + 1) % 4;
        _progress = _stationIndex * 0.25d;
        _phaseElapsed = 0d;
        _isPaused = true;
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        base.Render(canvas, dimensions);

        var track = GetTrackBounds(dimensions);
        var angle = _progress * Math.PI * 2d - Math.PI / 2d;
        var rx = track.Width * 0.5f;
        var ry = track.Height * 0.5f;
        var cx = (track.X + track.Width / 2f) + (float)Math.Cos(angle) * rx;
        var cy = (track.Y + track.Height / 2f) + (float)Math.Sin(angle) * ry;

        // Tangent to an ellipse: dx/dt = -rx sin(t), dy/dt = ry cos(t).
        // The heading makes the top-down vehicle follow the course rather than slide around it.
        var dx = -rx * (float)Math.Sin(angle);
        var dy = ry * (float)Math.Cos(angle);
        var headingDegrees = (float)(Math.Atan2(dy, dx) * 180d / Math.PI);

        canvas.SaveState();
        canvas.Translate(cx, cy);
        canvas.Rotate(headingDegrees);

        switch (_state.Champion)
        {
            case Champion.Charioteer:
                DrawChariot(canvas);
                break;
            case Champion.SpacePilot:
                DrawShip(canvas);
                break;
            default:
                DrawCar(canvas);
                break;
        }

        canvas.RestoreState();
    }

    private static RectF GetTrackBounds(RectF dimensions)
    {
        var width = dimensions.Width * 0.62f;
        var height = dimensions.Height * 0.48f;

        return new RectF(
            dimensions.X + (dimensions.Width - width) / 2f,
            dimensions.Y + dimensions.Height * 0.20f,
            width,
            height);
    }

    private static double EaseInOut(double value) =>
        value < 0.5d
            ? 2d * value * value
            : 1d - Math.Pow(-2d * value + 2d, 2d) / 2d;

    private static void DrawCar(ICanvas canvas)
    {
        // Formula-style car viewed from above. The nose points to +X.
        canvas.FillColor = Color.FromArgb("#111827");
        canvas.FillRoundedRectangle(new RectF(-19, -17, 10, 12), 3);
        canvas.FillRoundedRectangle(new RectF(-19, 5, 10, 12), 3);
        canvas.FillRoundedRectangle(new RectF(9, -16, 10, 11), 3);
        canvas.FillRoundedRectangle(new RectF(9, 5, 10, 11), 3);

        canvas.FillColor = Color.FromArgb("#EF4444");
        canvas.FillRoundedRectangle(new RectF(-22, -9, 42, 18), 6);
        canvas.FillRectangle(-26, -13, 7, 26);
        canvas.FillRectangle(15, -15, 5, 30);

        canvas.FillColor = Color.FromArgb("#F8FAFC");
        canvas.FillRoundedRectangle(new RectF(-5, -6, 13, 12), 5);

        canvas.FillColor = Color.FromArgb("#F8D866");
        canvas.FillEllipse(12, -3, 6, 6);
    }

    private static void DrawChariot(ICanvas canvas)
    {
        // Chariot and two horses viewed from above. Travel direction is +X.
        canvas.StrokeColor = Color.FromArgb("#F8D866");
        canvas.StrokeSize = 3;
        canvas.DrawLine(-3, -8, 17, -10);
        canvas.DrawLine(-3, 8, 17, 10);

        canvas.FillColor = Color.FromArgb("#B45309");
        canvas.FillRoundedRectangle(new RectF(-23, -12, 22, 24), 5);

        canvas.FillColor = Color.FromArgb("#1E293B");
        canvas.FillEllipse(-19, -18, 10, 10);
        canvas.FillEllipse(-19, 8, 10, 10);

        canvas.FillColor = Color.FromArgb("#F8D866");
        canvas.FillEllipse(-12, -5, 9, 9);

        canvas.FillColor = Color.FromArgb("#A16207");
        canvas.FillRoundedRectangle(new RectF(14, -15, 21, 10), 5);
        canvas.FillRoundedRectangle(new RectF(14, 5, 21, 10), 5);
        canvas.FillEllipse(29, -17, 8, 8);
        canvas.FillEllipse(29, 9, 8, 8);
    }

    private static void DrawShip(ICanvas canvas)
    {
        // Compact spacecraft viewed from above. Nose points to +X.
        canvas.FillColor = Color.FromArgb("#F97316");
        canvas.FillRectangle(-29, -7, 11, 5);
        canvas.FillRectangle(-29, 2, 11, 5);

        canvas.FillColor = Color.FromArgb("#6366F1");
        var body = new PathF();
        body.MoveTo(30, 0);
        body.LineTo(-16, -14);
        body.LineTo(-7, 0);
        body.LineTo(-16, 14);
        body.Close();
        canvas.FillPath(body);

        canvas.FillColor = Color.FromArgb("#818CF8");
        var wings = new PathF();
        wings.MoveTo(3, 0);
        wings.LineTo(-17, -24);
        wings.LineTo(-12, -4);
        wings.LineTo(-17, 24);
        wings.Close();
        canvas.FillPath(wings);

        canvas.FillColor = Color.FromArgb("#22D3EE");
        canvas.FillEllipse(2, -6, 12, 12);
    }
}
