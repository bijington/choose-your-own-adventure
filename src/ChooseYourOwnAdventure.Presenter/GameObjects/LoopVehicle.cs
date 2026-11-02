using ChooseYourOwnAdventure.Input;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.GameObjects;

/// <summary>
/// A small set of deliberately different control models used to demonstrate how
/// input changes game state during Update and becomes visible during Render.
/// </summary>
public sealed class LoopVehicle : GameObject
{
    private const double DriverAcceleration = 0.24d;
    private const double DriverDrag = 0.09d;
    private const double DriverBrake = 0.42d;
    private const double DriverMaxSpeed = 0.36d;
    private const double DriverSteeringRate = 1.45d;

    private const double ChariotPull = 0.16d;
    private const double ChariotDrag = 0.018d;
    private const double ChariotBrake = 0.30d;
    private const double ChariotMaxSpeed = 0.30d;

    private const double SpaceThrust = 0.92d;
    private const double SpaceGravity = 0.58d;
    private const double SpaceDownThrust = 0.80d;
    private const double SpaceMaxVelocity = 0.72d;

    private readonly AdventureState _state;
    private readonly AdventureInputRouter _input;

    private double _progress;
    private double _speed;
    private double _laneOffset;

    // 0 = bottom of the flight lane, 1 = top.
    private double _altitude = 0.12d;
    private double _verticalVelocity;

    public LoopVehicle(AdventureState state, AdventureInputRouter input)
    {
        _state = state;
        _input = input;
    }

    public bool IsRacingDriver => _state.Champion == Champion.RacingDriver;
    public bool IsCharioteer => _state.Champion == Champion.Charioteer;
    public bool IsSpacePilot => _state.Champion == Champion.SpacePilot;

    public bool IsAccelerating => !IsSpacePilot && _input.IsPressed(AdventureAction.Primary);
    public bool IsBraking => !IsSpacePilot &&
        (_input.IsPressed(AdventureAction.Secondary) || _input.IsPressed(AdventureAction.Down));

    public double SteeringInput => IsRacingDriver
        ? (_input.IsPressed(AdventureAction.Left) ? -1d : 0d) +
          (_input.IsPressed(AdventureAction.Right) ? 1d : 0d)
        : 0d;

    public bool IsSteering => Math.Abs(SteeringInput) > 0.001d;
    public double LaneOffset => _laneOffset;

    public bool IsPulling => IsCharioteer && IsAccelerating;
    public bool IsCoastingWithMomentum => IsCharioteer && !IsPulling && !IsBraking && _speed > 0.004d;

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
        }
        else if (IsCharioteer)
        {
            UpdateCharioteer(elapsed);
        }
        else
        {
            UpdateDriver(elapsed);
        }
    }

    private void UpdateDriver(double elapsed)
    {
        if (IsAccelerating)
        {
            _speed += DriverAcceleration * elapsed;
        }
        else
        {
            _speed -= DriverDrag * elapsed;
        }

        if (IsBraking)
        {
            _speed -= DriverBrake * elapsed;
        }

        _speed = Math.Clamp(_speed, 0d, DriverMaxSpeed);

        // Steering changes lateral position independently from forward velocity.
        // This makes the two inputs visibly affect different parts of game state.
        _laneOffset += SteeringInput * DriverSteeringRate * elapsed;
        _laneOffset = Math.Clamp(_laneOffset, -1d, 1d);

        _progress = (_progress + (_speed * elapsed)) % 1d;
    }

    private void UpdateCharioteer(double elapsed)
    {
        if (IsPulling)
        {
            _speed += ChariotPull * elapsed;
        }
        else
        {
            // Deliberately tiny drag: releasing the input does not stop the chariot.
            // Its current momentum continues to advance position every update.
            _speed -= ChariotDrag * elapsed;
        }

        if (IsBraking)
        {
            _speed -= ChariotBrake * elapsed;
        }

        _speed = Math.Clamp(_speed, 0d, ChariotMaxSpeed);
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
        var centreX = track.X + track.Width / 2f;
        var centreY = track.Y + track.Height / 2f;

        var cos = (float)Math.Cos(angle);
        var sin = (float)Math.Sin(angle);
        var cx = centreX + cos * rx;
        var cy = centreY + sin * ry;

        if (IsRacingDriver)
        {
            // Move across the width of the circuit while still following the oval.
            var lateralOffset = (float)_laneOffset * 16f;
            cx += cos * lateralOffset;
            cy += sin * lateralOffset;
        }

        var dx = -rx * sin;
        var dy = ry * cos;
        var headingDegrees = (float)(Math.Atan2(dy, dx) * 180d / Math.PI);

        canvas.SaveState();
        canvas.Translate(cx, cy);
        canvas.Rotate(headingDegrees);

        if (IsCharioteer)
        {
            DrawChariot(canvas);
        }
        else
        {
            DrawCar(canvas, (float)SteeringInput);
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

    private static void DrawCar(ICanvas canvas, float steering)
    {
        // Wheels visibly steer while the body continues along the track tangent.
        DrawWheel(canvas, -14, -13, steering * 18f);
        DrawWheel(canvas, -14, 7, steering * 18f);
        DrawWheel(canvas, 10, -13, 0f);
        DrawWheel(canvas, 10, 7, 0f);

        canvas.FillColor = Color.FromArgb("#EF4444");
        canvas.FillRoundedRectangle(new RectF(-22, -9, 42, 18), 6);
        canvas.FillRectangle(-26, -13, 7, 26);
        canvas.FillRectangle(15, -15, 5, 30);
        canvas.FillColor = Color.FromArgb("#F8FAFC");
        canvas.FillRoundedRectangle(new RectF(-5, -6, 13, 12), 5);
    }

    private static void DrawWheel(ICanvas canvas, float x, float y, float rotation)
    {
        canvas.SaveState();
        canvas.Translate(x + 5, y + 5);
        canvas.Rotate(rotation);
        canvas.FillColor = Color.FromArgb("#111827");
        canvas.FillRoundedRectangle(new RectF(-5, -6, 10, 12), 3);
        canvas.RestoreState();
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
