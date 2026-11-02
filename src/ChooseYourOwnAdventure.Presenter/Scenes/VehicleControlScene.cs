using ChooseYourOwnAdventure.GameObjects;
using ChooseYourOwnAdventure.Input;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class VehicleControlScene : GameScene
{
    private readonly AdventureState _state;
    private readonly LoopVehicle _vehicle;

    public VehicleControlScene(AdventureState state, AdventureInputRouter input)
    {
        _state = state;
        _state.CurrentLocation = state.Champion switch
        {
            Champion.SpacePilot => "Flight School",
            Champion.Charioteer => "The Grand Arena",
            _ => "Behind the Wheel"
        };

        _vehicle = new LoopVehicle(state, input);
        Add(_vehicle);
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawBackground(canvas, dimensions);
        DrawHeading(canvas, dimensions);
        DrawCodePanel(canvas, dimensions, _vehicle);
        DrawCourse(canvas, dimensions);
        DrawInstructions(canvas, dimensions, _vehicle);
        DrawNextPrompt(canvas, dimensions);

        // The actual Orbit GameObject is rendered over the shell/course.
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
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Colors.White;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.031f, 24f, 46f);
        canvas.DrawString(GetHeading(), dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.05f, dimensions.Width - 80, 52,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.014f, 13f, 21f);
        canvas.DrawString(GetSubtitle(), dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.11f, dimensions.Width - 80, 34,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawCourse(ICanvas canvas, RectF dimensions)
    {
        if (_state.Champion == Champion.SpacePilot)
        {
            DrawFlightLane(canvas, dimensions);
            return;
        }

        DrawTrack(canvas, dimensions);
    }

    private void DrawTrack(ICanvas canvas, RectF dimensions)
    {
        var track = VehicleControlLayout.GetTrackBounds(dimensions);

        if (_state.Champion == Champion.Charioteer)
        {
            canvas.StrokeColor = Color.FromArgb("#92400E");
            canvas.StrokeSize = 42;
            canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
            canvas.StrokeColor = Color.FromArgb("#F8D866");
            canvas.StrokeSize = 2;
            canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
            return;
        }

        // Wider circuit with visible lane boundaries so steering has an obvious effect.
        canvas.StrokeColor = Color.FromArgb("#334155");
        canvas.StrokeSize = 54;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);

        canvas.StrokeColor = Color.FromArgb("#CBD5E1");
        canvas.StrokeSize = 2;
        canvas.DrawEllipse(track.X - 24, track.Y - 24, track.Width + 48, track.Height + 48);
        canvas.DrawEllipse(track.X + 24, track.Y + 24,
            Math.Max(1, track.Width - 48), Math.Max(1, track.Height - 48));

        canvas.StrokeColor = Color.FromArgb("#64748B");
        canvas.StrokeSize = 1;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
    }

    private static void DrawFlightLane(ICanvas canvas, RectF dimensions)
    {
        var lane = VehicleControlLayout.GetFlightLane(dimensions);

        canvas.FillColor = Color.FromArgb("#0B1026");
        canvas.FillRoundedRectangle(lane, 24);
        canvas.StrokeColor = Color.FromArgb("#312E81");
        canvas.StrokeSize = 3;
        canvas.DrawRoundedRectangle(lane, 24);

        canvas.StrokeColor = Color.FromArgb("#6366F1");
        canvas.StrokeSize = 2;
        for (var i = 1; i < 5; i++)
        {
            var y = lane.Bottom - lane.Height * (i / 5f);
            canvas.DrawLine(lane.X + 16, y, lane.Right - 16, y);
        }

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#64748B");
        canvas.FontSize = 12;
        canvas.DrawString("ALTITUDE", lane.X, lane.Y - 28, lane.Width, 20,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawCodePanel(ICanvas canvas, RectF dimensions, LoopVehicle vehicle)
    {
        var panel = VehicleControlLayout.GetCodePanel(dimensions);
        canvas.FillColor = Color.FromArgb("#0F172A");
        canvas.FillRoundedRectangle(panel, 18);
        canvas.StrokeColor = Color.FromArgb("#334155");
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(panel, 18);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = Math.Clamp(panel.Width * 0.042f, 13f, 19f);
        canvas.DrawString("LoopVehicle.cs", panel.X + 22, panel.Y + 14,
            panel.Width - 44, 26, HorizontalAlignment.Left, VerticalAlignment.Center);

        if (vehicle.IsSpacePilot)
        {
            DrawSpaceCode(canvas, panel, vehicle);
        }
        else if (vehicle.IsCharioteer)
        {
            DrawChariotCode(canvas, panel, vehicle);
        }
        else
        {
            DrawDriverCode(canvas, panel, vehicle);
        }
    }

    private static void DrawDriverCode(ICanvas canvas, RectF panel, LoopVehicle vehicle)
    {
        var y = panel.Y + 58;
        DrawCodeSection(canvas, panel, ref y, "INPUT",
            vehicle.IsAccelerating || vehicle.IsBraking || vehicle.IsSteering,
        [
            new("var throttle = input.IsPressed(Primary);", vehicle.IsAccelerating),
            new("var steer = input.Horizontal;", vehicle.IsSteering)
        ]);

        y += 14;
        DrawCodeSection(canvas, panel, ref y, "UPDATE",
            vehicle.IsAccelerating || vehicle.IsBraking || vehicle.IsSteering || vehicle.IsMoving,
        [
            new("speed += acceleration * elapsed;", vehicle.IsAccelerating),
            new("lane += steer * steeringRate * elapsed;", vehicle.IsSteering),
            new("position += speed * elapsed;", vehicle.IsMoving)
        ]);

        y += 14;
        DrawCodeSection(canvas, panel, ref y, "RENDER", vehicle.IsMoving || vehicle.IsSteering,
        [
            new("DrawCar(canvas, position, lane);", vehicle.IsMoving || vehicle.IsSteering)
        ]);

        DrawMetric(canvas, panel,
            $"speed = {vehicle.Speed:0.000}   lane = {vehicle.LaneOffset:+0.00;-0.00;0.00}");
    }

    private static void DrawChariotCode(ICanvas canvas, RectF panel, LoopVehicle vehicle)
    {
        var y = panel.Y + 58;
        DrawCodeSection(canvas, panel, ref y, "INPUT", vehicle.IsPulling || vehicle.IsBraking,
        [
            new("var pull = input.IsPressed(Primary);", vehicle.IsPulling),
            new("var brake = input.IsPressed(Secondary);", vehicle.IsBraking)
        ]);

        y += 14;
        DrawCodeSection(canvas, panel, ref y, "UPDATE", true,
        [
            new("if (pull) speed += force * elapsed;", vehicle.IsPulling),
            new("else speed -= smallDrag * elapsed;", vehicle.IsCoastingWithMomentum),
            new("position += speed * elapsed;", vehicle.IsMoving)
        ]);

        y += 14;
        DrawCodeSection(canvas, panel, ref y, "RENDER", vehicle.IsMoving,
        [
            new("DrawChariot(canvas, position);", vehicle.IsMoving)
        ]);

        DrawMetric(canvas, panel, $"momentum / speed = {vehicle.Speed:0.000}");
    }

    private static void DrawSpaceCode(ICanvas canvas, RectF panel, LoopVehicle vehicle)
    {
        var y = panel.Y + 58;

        DrawCodeSection(canvas, panel, ref y, "INPUT", vehicle.IsThrusting || vehicle.IsUsingDownThrust,
        [
            new("var thrust = input.IsPressed(Primary);", vehicle.IsThrusting),
            new("var down = input.IsPressed(Secondary);", vehicle.IsUsingDownThrust)
        ]);

        y += 14;
        DrawCodeSection(canvas, panel, ref y, "UPDATE", true,
        [
            new("if (thrust)", vehicle.IsThrusting),
            new("    velocityY -= thrustPower * elapsed;", vehicle.IsThrusting),
            new("else", !vehicle.IsThrusting),
            new("    velocityY += gravity * elapsed;", !vehicle.IsThrusting),
            new("positionY += velocityY * elapsed;", vehicle.IsMoving)
        ]);

        y += 14;
        DrawCodeSection(canvas, panel, ref y, "RENDER", vehicle.IsMoving || vehicle.IsThrusting,
        [
            new("DrawShip(canvas, positionY);", vehicle.IsMoving || vehicle.IsThrusting)
        ]);

        DrawMetric(canvas, panel,
            $"altitude = {vehicle.Altitude:0.00}   velocityY = {vehicle.VerticalVelocity:+0.00;-0.00;0.00}");
    }

    private static void DrawCodeSection(ICanvas canvas, RectF panel, ref float y,
        string title, bool active, IReadOnlyList<CodeLine> lines)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = active ? Color.FromArgb("#F8D866") : Color.FromArgb("#64748B");
        canvas.FontSize = Math.Clamp(panel.Width * 0.035f, 12f, 17f);
        canvas.DrawString(title, panel.X + 22, y, panel.Width - 44, 24,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        y += 28;

        foreach (var line in lines)
        {
            canvas.Font = line.IsActive
                ? Microsoft.Maui.Graphics.Font.DefaultBold
                : Microsoft.Maui.Graphics.Font.Default;
            canvas.FontColor = line.IsActive ? Colors.White : Color.FromArgb("#94A3B8");
            canvas.FontSize = Math.Clamp(panel.Width * 0.027f, 10f, 14f);
            canvas.DrawString(line.Text, panel.X + 30, y, panel.Width - 52, 24,
                HorizontalAlignment.Left, VerticalAlignment.Center);
            y += 25;
        }
    }

    private static void DrawMetric(ICanvas canvas, RectF panel, string text)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#F8D866");
        canvas.FontSize = Math.Clamp(panel.Width * 0.031f, 11f, 15f);
        canvas.DrawString(text, panel.X + 22, panel.Bottom - 42,
            panel.Width - 44, 24, HorizontalAlignment.Left, VerticalAlignment.Center);
    }

    private void DrawInstructions(ICanvas canvas, RectF dimensions, LoopVehicle vehicle)
    {
        var track = VehicleControlLayout.GetTrackBounds(dimensions);
        string text;

        if (vehicle.IsSpacePilot)
        {
            text = vehicle.IsThrusting
                ? "THRUST ON — velocity changes upward while the button is held"
                : vehicle.IsUsingDownThrust
                    ? "DOWN THRUST — applying force with the secondary input"
                    : vehicle.IsFalling
                        ? "BUTTON RELEASED — gravity is changing velocity every update"
                        : "HOLD A / ENTER FOR THRUST   ·   RELEASE TO FALL   ·   B / SHIFT DOWN THRUST";
        }
        else if (vehicle.IsCharioteer)
        {
            text = vehicle.IsPulling
                ? "PULLING — the horses add force and build momentum"
                : vehicle.IsBraking
                    ? "REINS ON — braking removes momentum"
                    : vehicle.IsCoastingWithMomentum
                        ? "BUTTON RELEASED — momentum keeps the chariot moving"
                        : "HOLD A / ENTER TO BUILD MOMENTUM   ·   HOLD B / SHIFT TO BRAKE";
        }
        else
        {
            text = vehicle.IsSteering
                ? "STEERING — LEFT / RIGHT changes lane state while velocity keeps moving the car"
                : vehicle.IsAccelerating
                    ? "THROTTLE — speed changes during UPDATE"
                    : vehicle.IsBraking
                        ? "BRAKING — input is reducing velocity"
                        : vehicle.IsMoving
                            ? "COASTING — UPDATE keeps advancing position"
                            : "HOLD A / ENTER FOR THROTTLE   ·   LEFT / RIGHT TO STEER   ·   B / SHIFT TO BRAKE";
        }

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = vehicle.IsAccelerating || vehicle.IsBraking || vehicle.IsThrusting ||
                           vehicle.IsUsingDownThrust || vehicle.IsSteering
            ? Color.FromArgb("#F8D866")
            : Color.FromArgb("#CBD5E1");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.012f, 11f, 17f);
        canvas.DrawString(text, track.X - 10, track.Bottom + 56, track.Width + 20, 42,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private static void DrawNextPrompt(ICanvas canvas, RectF dimensions)
    {
        var width = Math.Clamp(dimensions.Width * 0.19f, 190f, 300f);
        var bounds = new RectF(dimensions.Right - width - 44, dimensions.Bottom - 58, width, 34);
        canvas.FillColor = Color.FromArgb("#172033");
        canvas.FillRoundedRectangle(bounds, 12);
        canvas.StrokeColor = Color.FromArgb("#F8D866");
        canvas.StrokeSize = 1.5f;
        canvas.DrawRoundedRectangle(bounds, 12);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#F8D866");
        canvas.FontSize = 12;
        canvas.DrawString("W / STICK UP  ·  AUDIENCE RACE →", bounds.X + 10, bounds.Y, bounds.Width - 20, bounds.Height,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private string GetHeading() => _state.Champion switch
    {
        Champion.SpacePilot => "NOW FLY THE LOOP",
        Champion.Charioteer => "NOW FEEL THE MOMENTUM",
        _ => "NOW DRIVE THE LOOP"
    };

    private string GetSubtitle() => _state.Champion switch
    {
        Champion.Charioteer => "Build speed, release the input, and notice that state keeps carrying the chariot forward.",
        Champion.SpacePilot => "Hold for thrust. Release the button and gravity takes over on the very next update.",
        _ => "Throttle changes velocity. Left and right change steering. Both become visible through the same game loop."
    };

    private readonly record struct CodeLine(string Text, bool IsActive);
}
