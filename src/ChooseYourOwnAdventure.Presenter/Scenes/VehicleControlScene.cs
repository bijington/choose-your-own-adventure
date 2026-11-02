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
        _state.CurrentLocation = "Behind the Wheel";
        _vehicle = new LoopVehicle(state, input);
        Add(_vehicle);
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawBackground(canvas, dimensions);
        DrawHeading(canvas, dimensions);
        DrawCodePanel(canvas, dimensions, _vehicle);
        DrawTrack(canvas, dimensions);
        DrawInstructions(canvas, dimensions, _vehicle);

        // The actual Orbit GameObject is rendered over the shell/track.
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
        canvas.DrawString("NOW DRIVE THE LOOP", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.05f, dimensions.Width - 80, 52,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.014f, 13f, 21f);
        canvas.DrawString(GetSubtitle(), dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.11f, dimensions.Width - 80, 34,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawTrack(ICanvas canvas, RectF dimensions)
    {
        var track = VehicleControlLayout.GetTrackBounds(dimensions);

        switch (_state.Champion)
        {
            case Champion.Charioteer:
                canvas.StrokeColor = Color.FromArgb("#92400E");
                canvas.StrokeSize = 36;
                canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
                canvas.StrokeColor = Color.FromArgb("#F8D866");
                canvas.StrokeSize = 2;
                canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
                break;
            case Champion.SpacePilot:
                canvas.StrokeColor = Color.FromArgb("#312E81");
                canvas.StrokeSize = 38;
                canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
                canvas.StrokeColor = Color.FromArgb("#6366F1");
                canvas.StrokeSize = 7;
                canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
                canvas.StrokeColor = Color.FromArgb("#22D3EE");
                canvas.StrokeSize = 2;
                canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
                break;
            default:
                canvas.StrokeColor = Color.FromArgb("#475569");
                canvas.StrokeSize = 36;
                canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
                canvas.StrokeColor = Color.FromArgb("#CBD5E1");
                canvas.StrokeSize = 2;
                canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
                break;
        }
    }

    private static void DrawCodePanel(ICanvas canvas, RectF dimensions, LoopVehicle vehicle)
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

        var y = panel.Y + 58;
        DrawCodeSection(canvas, panel, ref y, "INPUT", vehicle.IsAccelerating || vehicle.IsBraking,
        [
            "var accelerate = input.IsPressed(Primary);",
            "var brake = input.IsPressed(Secondary);"
        ]);

        y += 16;
        DrawCodeSection(canvas, panel, ref y, "UPDATE", vehicle.IsAccelerating || vehicle.IsBraking || vehicle.IsMoving,
        [
            "speed += acceleration * elapsed;",
            "position += speed * elapsed;"
        ]);

        y += 16;
        DrawCodeSection(canvas, panel, ref y, "RENDER", vehicle.IsMoving,
        [
            "DrawVehicle(canvas, position);"
        ]);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#F8D866");
        canvas.FontSize = Math.Clamp(panel.Width * 0.035f, 12f, 17f);
        canvas.DrawString($"speed = {vehicle.Speed:0.000}", panel.X + 22, panel.Bottom - 42,
            panel.Width - 44, 24, HorizontalAlignment.Left, VerticalAlignment.Center);
    }

    private static void DrawCodeSection(ICanvas canvas, RectF panel, ref float y,
        string title, bool active, IReadOnlyList<string> lines)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = active ? Color.FromArgb("#F8D866") : Color.FromArgb("#64748B");
        canvas.FontSize = Math.Clamp(panel.Width * 0.035f, 12f, 17f);
        canvas.DrawString(title, panel.X + 22, y, panel.Width - 44, 24,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        y += 28;

        foreach (var line in lines)
        {
            canvas.Font = active
                ? Microsoft.Maui.Graphics.Font.DefaultBold
                : Microsoft.Maui.Graphics.Font.Default;
            canvas.FontColor = active ? Colors.White : Color.FromArgb("#94A3B8");
            canvas.FontSize = Math.Clamp(panel.Width * 0.027f, 10f, 14f);
            canvas.DrawString(line, panel.X + 30, y, panel.Width - 52, 24,
                HorizontalAlignment.Left, VerticalAlignment.Center);
            y += 27;
        }
    }

    private static void DrawInstructions(ICanvas canvas, RectF dimensions, LoopVehicle vehicle)
    {
        var track = VehicleControlLayout.GetTrackBounds(dimensions);
        var text = vehicle.IsAccelerating
            ? "ACCELERATING — INPUT and UPDATE are active"
            : vehicle.IsBraking
                ? "BRAKING — input is changing the game state"
                : vehicle.IsMoving
                    ? "COASTING — UPDATE keeps advancing position"
                    : "HOLD A / ENTER TO ACCELERATE   ·   HOLD B / SHIFT TO BRAKE";

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = vehicle.IsAccelerating || vehicle.IsBraking
            ? Color.FromArgb("#F8D866")
            : Color.FromArgb("#CBD5E1");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.012f, 11f, 17f);
        canvas.DrawString(text, track.X - 10, track.Bottom + 42, track.Width + 20, 32,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private string GetSubtitle() => _state.Champion switch
    {
        Champion.Charioteer => "Your champion has the reins. Hold the button and watch input become motion.",
        Champion.SpacePilot => "Thrusters online. Hold the button and watch input become motion.",
        _ => "Your champion is behind the wheel. Hold the button and watch input become motion."
    };
}
