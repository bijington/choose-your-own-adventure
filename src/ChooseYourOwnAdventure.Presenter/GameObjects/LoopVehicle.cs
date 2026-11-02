using ChooseYourOwnAdventure.Input;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.GameObjects;

/// <summary>
/// A deliberately small, themed input demo.
/// Driver and Charioteer move around a course using acceleration/braking.
/// Space Pilot uses thrust and gravity so releasing the button changes the motion immediately.
/// </summary>
public sealed class LoopVehicle : GameObject
{
    private const double Acceleration = 0.22d;
    private const double Drag = 0.08d;
    private const double Brake = 0.38d;
    private const double MaxSpeed = 0.34d;

    private const double SpaceThrust = 0.92d;
    private const double SpaceGravity = 0.58d;
    private const double SpaceDownThrust = 0.80d;
    private const double SpaceMaxVelocity = 0.72d;

    private readonly AdventureState _state;
    private readonly AdventureInputRouter _input;

    private double _progress;
    private double _speed;

    // 0 = bottom of the flight lane, 1 = top.
    private double _altitude = 0.12d;
    private double _verticalVelocity;

    public LoopVehicle(AdventureState state, AdventureInputRouter input)
    {
        _state = state;
        _input = input;
    }

    public bool IsSpacePilot => _state.Champion == Champion.SpacePilot;
    public bool IsAccelerating => !IsSpacePilot && _input.IsPressed(AdventureAction.Primary);
    public bool IsBraking => !IsSpacePilot &&
        (_input.IsPressed(AdventureAction.Secondary) || _input.IsPressed(AdventureAction.Down));

    public bool IsThrusting => IsSpacePilot && _input.IsPressed(AdventureAction.Primary);
    public bool IsUsingDownThrust => IsSpacePilot &&
        (_input.IsPressed(AdventureAction.Secondary) || _input.IsPressed(AdventureAction.Down));
    public bool IsFalling => IsSpacePilot && !IsThrusting && _verticalVelocity > 0.0005d;

    public bool IsMoving => IsSpacePilot
        ? Math.Abs(_verticalVelocity) > 0.0005d
        : _speed > 0.0005d;

    public double Speed => _speed;
    public double Altitude => _altitude;
    public double VerticalVelocity => _verticalVelocity;

    public override void Update(double millisecondsSinceLastUpdate)
    {
        base.Update(millisecondsSinceLastUpdate);

        var elapsed = millisecondsSinceLastUpdate / 1000d;

        if (IsSpacePilot)
        {
            UpdateSpacePilot(elapsed);
            return;
        }

        UpdateCourseVehicle(elapsed);
    }

    private void UpdateCourseVehicle(double elapsed)
    {
        if (IsAccelerating)
        {
            _speed += Acceleration * elapsed;
        }
        else
        {
            _speed -= Drag * elapsed;
        }

        if (IsBraking)
        {
            _speed -= Brake * elapsed;
        }

        _speed = Math.Clamp(_speed, 0d, MaxSpeed);
        _progress = (_progress + (_speed * elapsed)) % 1d;
    }

    private void UpdateSpacePilot(double elapsed)
    {
        // Screen Y grows downward, so negative velocity means "up".
        if (IsThrusting)
        {
            _verticalVelocity -= SpaceThrust * elapsed;
        }
        else
        {
            _verticalVelocity += SpaceGravity * elapsed;
        }

        if (IsUsingDownThrust)
        {
            _verticalVelocity += SpaceDownThrust * elapsed;
        }

        _verticalVelocity = Math.Clamp(_verticalVelocity, -SpaceMaxVelocity, SpaceMaxVelocity);
        _altitude -= _verticalVelocity * elapsed;

        if (_altitude <= 0d)
        {
            _altitude = 0d;
            _verticalVelocity = Math.Min(0d, _verticalVelocity);
        }
        else if (_altitude >= 1d)
        {
            _altitude = 1d;
            _verticalVelocity = Math.Max(0d, _verticalVelocity);
        }
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        base.Render(canvas, dimensions);

        if (IsSpacePilot)
        {
            RenderSpacePilot(canvas, dimensions);
            return;
        }

        RenderCourseVehicle(canvas, dimensions);
    }

    private void RenderCourseVehicle(ICanvas canvas, RectF dimensions)
    {
        var track = VehicleControlLayout.GetTrackBounds(dimensions);
        var angle = _progress * Math.PI * 2d - Math.PI / 2d;
        var rx = track.Width * 0.5f;
        var ry = track.Height * 0.5f;
        var cx = (track.X + track.Width / 2f) + (float)Math.Cos(angle) * rx;
        var cy = (track.Y + track.Height / 2f) + (float)Math.Sin(angle) * ry;
        var dx = -rx * (float)Math.Sin(angle);
        var dy = ry * (float)Math.Cos(angle);
        var headingDegrees = (float)(Math.Atan2(dy, dx) * 180d / Math.PI);

        canvas.SaveState();
        canvas.Translate(cx, cy);
        canvas.Rotate(headingDegrees);

        if (_state.Champion == Champion.Charioteer)
        {
            DrawChariot(canvas);
        }
        else
        {
            DrawCar(canvas);
        }

        canvas.RestoreState();
    }

    private void RenderSpacePilot(ICanvas canvas, RectF dimensions)
    {
        var lane = VehicleControlLayout.GetFlightLane(dimensions);
        var x = lane.X + lane.Width / 2f;
        var bottom = lane.Bottom - 34f;
        var top = lane.Y + 34f;
        var y = bottom - (float)_altitude * (bottom - top);

        canvas.SaveState();
        canvas.Translate(x, y);
        canvas.Rotate(-90f);
        DrawShip(canvas, IsThrusting);
        canvas.RestoreState();
    }

    private static void DrawCar(ICanvas canvas)
    {
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
    }

    private static void DrawChariot(ICanvas canvas)
    {
        canvas.StrokeColor = Color.FromArgb("#F8D866");
        canvas.StrokeSize = 3;
        canvas.DrawLine(-3, -8, 17, -10);
        canvas.DrawLine(-3, 8, 17, 10);
        canvas.FillColor = Color.FromArgb("#B45309");
        canvas.FillRoundedRectangle(new RectF(-23, -12, 22, 24), 5);
        canvas.FillColor = Color.FromArgb("#1E293B");
        canvas.FillEllipse(-19, -18, 10, 10);
        canvas.FillEllipse(-19, 8, 10, 10);
        canvas.FillColor = Color.FromArgb("#A16207");
        canvas.FillRoundedRectangle(new RectF(14, -15, 21, 10), 5);
        canvas.FillRoundedRectangle(new RectF(14, 5, 21, 10), 5);
        canvas.FillEllipse(29, -17, 8, 8);
        canvas.FillEllipse(29, 9, 8, 8);
    }

    private static void DrawShip(ICanvas canvas, bool thrusting)
    {
        if (thrusting)
        {
            canvas.FillColor = Color.FromArgb("#FBBF24");
            canvas.FillRectangle(-37, -6, 18, 4);
            canvas.FillRectangle(-37, 2, 18, 4);
            canvas.FillColor = Color.FromArgb("#F97316");
            canvas.FillRectangle(-31, -8, 12, 5);
            canvas.FillRectangle(-31, 3, 12, 5);
        }
        else
        {
            canvas.FillColor = Color.FromArgb("#F97316");
            canvas.FillRectangle(-27, -6, 8, 4);
            canvas.FillRectangle(-27, 2, 8, 4);
        }

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

public static class VehicleControlLayout
{
    public static RectF GetCodePanel(RectF dimensions)
    {
        var margin = Math.Max(36f, dimensions.Width * 0.04f);
        var gap = Math.Max(24f, dimensions.Width * 0.03f);
        var top = dimensions.Y + dimensions.Height * 0.16f;
        var height = dimensions.Height * 0.70f;
        var width = (dimensions.Width - margin * 2 - gap) * 0.43f;
        return new RectF(dimensions.X + margin, top, width, height);
    }

    public static RectF GetTrackBounds(RectF dimensions)
    {
        var code = GetCodePanel(dimensions);
        var margin = Math.Max(36f, dimensions.Width * 0.04f);
        var gap = Math.Max(24f, dimensions.Width * 0.03f);
        var rightX = code.Right + gap;
        var rightWidth = dimensions.Right - margin - rightX;
        var rightTop = dimensions.Y + dimensions.Height * 0.24f;
        var rightHeight = dimensions.Height * 0.48f;
        return new RectF(rightX + rightWidth * 0.06f, rightTop, rightWidth * 0.88f, rightHeight);
    }

    public static RectF GetFlightLane(RectF dimensions)
    {
        var track = GetTrackBounds(dimensions);
        return new RectF(
            track.X + track.Width * 0.28f,
            track.Y - track.Height * 0.12f,
            track.Width * 0.44f,
            track.Height * 1.24f);
    }
}
