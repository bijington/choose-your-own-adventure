using ChooseYourOwnAdventure.GameObjects;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class GameLoopScene : GameScene
{
    private readonly AdventureState _state;
    private readonly LoopVehicle _vehicle;

    public GameLoopScene(AdventureState state)
    {
        _state = state;
        _state.CurrentLocation = GetLocationName(state.Champion);
        _vehicle = new LoopVehicle(state);
        Add(_vehicle);
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawBackground(canvas, dimensions);
        DrawHeading(canvas, dimensions);
        DrawTrack(canvas, dimensions);
        DrawStations(canvas, dimensions, _vehicle);

        if (_state.GameLoopStage == GameLoopStage.FirmwareComparison)
        {
            DrawFirmwareComparison(canvas, dimensions);
        }
        else if (_state.GameLoopStage == GameLoopStage.Running)
        {
            DrawOrbitExplanation(canvas, dimensions, _vehicle.CurrentStation);
        }
        else
        {
            DrawOverview(canvas, dimensions, _vehicle.CurrentStation);
        }

        // Orbit renders GameObjects after the scene shell. The LoopVehicle that moves around
        // the course is therefore drawn on top of the track and station markers.
        base.Render(canvas, dimensions);
    }

    public static RectF GetTrackBounds(RectF dimensions)
    {
        var width = dimensions.Width * 0.62f;
        var height = dimensions.Height * 0.48f;

        return new RectF(
            dimensions.X + (dimensions.Width - width) / 2f,
            dimensions.Y + dimensions.Height * 0.20f,
            width,
            height);
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
        canvas.DrawString(GetTitle(), dimensions.X + 40, dimensions.Y + dimensions.Height * 0.055f,
            Math.Max(0, dimensions.Width - 80), 52, HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.014f, 13f, 21f);
        canvas.DrawString("A game keeps moving even when nobody clicks a button.", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.115f, Math.Max(0, dimensions.Width - 80), 36,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawTrack(ICanvas canvas, RectF dimensions)
    {
        var track = GetTrackBounds(dimensions);

        // The lesson is identical whichever champion was chosen, but the first audience choice
        // changes the visual language of the course for the rest of the talk.
        switch (_state.Champion)
        {
            case Champion.Charioteer:
                DrawArenaTrack(canvas, track);
                break;
            case Champion.SpacePilot:
                DrawOrbitalTrack(canvas, track);
                break;
            default:
                DrawRaceTrack(canvas, track);
                break;
        }
    }

    private static void DrawRaceTrack(ICanvas canvas, RectF track)
    {
        canvas.StrokeColor = Color.FromArgb("#475569");
        canvas.StrokeSize = 30;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);

        canvas.StrokeColor = Color.FromArgb("#94A3B8");
        canvas.StrokeSize = 2;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);

        var inner = Inset(track, 9);
        canvas.StrokeColor = Color.FromArgb("#1E293B");
        canvas.StrokeSize = 2;
        canvas.DrawEllipse(inner.X, inner.Y, inner.Width, inner.Height);
    }

    private static void DrawArenaTrack(ICanvas canvas, RectF track)
    {
        canvas.StrokeColor = Color.FromArgb("#92400E");
        canvas.StrokeSize = 34;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);

        canvas.StrokeColor = Color.FromArgb("#D97706");
        canvas.StrokeSize = 23;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);

        canvas.StrokeColor = Color.FromArgb("#F8D866");
        canvas.StrokeSize = 2;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
    }

    private static void DrawOrbitalTrack(ICanvas canvas, RectF track)
    {
        canvas.StrokeColor = Color.FromArgb("#312E81");
        canvas.StrokeSize = 35;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);

        canvas.StrokeColor = Color.FromArgb("#6366F1");
        canvas.StrokeSize = 7;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);

        canvas.StrokeColor = Color.FromArgb("#22D3EE");
        canvas.StrokeSize = 2;
        canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
    }

    private static RectF Inset(RectF rect, float amount) =>
        new(rect.X + amount, rect.Y + amount,
            Math.Max(0, rect.Width - amount * 2), Math.Max(0, rect.Height - amount * 2));

    private static void DrawStations(ICanvas canvas, RectF dimensions, LoopVehicle vehicle)
    {
        var track = GetTrackBounds(dimensions);
        var stations = new[]
        {
            (LoopStation.Input, "INPUT", (track.X + track.Width / 2f), track.Y),
            (LoopStation.Update, "UPDATE", track.Right, (track.Y + track.Height / 2f)),
            (LoopStation.Render, "RENDER", (track.X + track.Width / 2f), track.Bottom),
            (LoopStation.Wait, "WAIT", track.X, (track.Y + track.Height / 2f))
        };

        foreach (var (station, label, x, y) in stations)
        {
            var isActive = vehicle.IsPausedAtStation && vehicle.CurrentStation == station;
            var radius = isActive ? 29f + vehicle.StationPulse * 4f : 27f;

            if (isActive)
            {
                canvas.FillColor = Color.FromRgba(248, 216, 102, (int)(45 + 60 * vehicle.StationPulse));
                canvas.FillEllipse(x - radius - 7, y - radius - 7, (radius + 7) * 2, (radius + 7) * 2);
            }

            canvas.FillColor = isActive
                ? Color.FromArgb("#1F2937")
                : Color.FromArgb("#111827");
            canvas.FillEllipse(x - radius, y - radius, radius * 2, radius * 2);

            canvas.StrokeColor = Color.FromArgb("#F8D866");
            canvas.StrokeSize = isActive ? 4 : 3;
            canvas.DrawEllipse(x - radius, y - radius, radius * 2, radius * 2);

            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontColor = isActive ? Color.FromArgb("#F8D866") : Colors.White;
            canvas.FontSize = 13;
            canvas.DrawString(label, x - 50, y - 11, 100, 22,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }

    private static void DrawOverview(ICanvas canvas, RectF dimensions, LoopStation station)
    {
        DrawInfoPanel(canvas, dimensions,
            "BUSINESS APP",
            "event → command → state change → UI update → wait",
            $"GAME · {GetStationName(station)}",
            GetStationDescription(station));
    }

    private static void DrawFirmwareComparison(ICanvas canvas, RectF dimensions)
    {
        DrawInfoPanel(canvas, dimensions,
            "FIRMWARE SUPERLOOP",
            "while (true)\n{\n    ReadInputs();\n    UpdateState();\n    WriteOutputs();\n    Delay();\n}",
            "GAME LOOP",
            "while (running)\n{\n    Input();\n    Update();\n    Render();\n    Wait();\n}");
    }

    private static void DrawOrbitExplanation(ICanvas canvas, RectF dimensions, LoopStation station)
    {
        var (title, body, why) = GetOrbitExplanation(station);
        DrawInfoPanel(canvas, dimensions,
            title,
            body,
            "WHY IT MATTERS",
            why);
    }

    private static void DrawInfoPanel(ICanvas canvas, RectF dimensions,
        string leftTitle, string leftBody, string rightTitle, string rightBody)
    {
        var top = dimensions.Y + dimensions.Height * 0.74f;
        var width = dimensions.Width * 0.40f;
        var height = dimensions.Height * 0.17f;
        var gap = dimensions.Width * 0.04f;
        var left = new RectF((dimensions.X + dimensions.Width / 2f) - gap / 2f - width, top, width, height);
        var right = new RectF((dimensions.X + dimensions.Width / 2f) + gap / 2f, top, width, height);
        DrawInfoCard(canvas, left, leftTitle, leftBody);
        DrawInfoCard(canvas, right, rightTitle, rightBody);
    }

    private static void DrawInfoCard(ICanvas canvas, RectF rect, string title, string body)
    {
        canvas.FillColor = Color.FromArgb("#0F172A");
        canvas.FillRoundedRectangle(rect, 14);
        canvas.StrokeColor = Color.FromArgb("#334155");
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 14);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#F8D866");
        canvas.FontSize = Math.Clamp(rect.Width * 0.036f, 12f, 18f);
        canvas.DrawString(title, rect.X + 16, rect.Y + 12, rect.Width - 32, 24,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#E2E8F0");
        canvas.FontSize = Math.Clamp(rect.Width * 0.029f, 11f, 15f);
        canvas.DrawString(body, rect.X + 16, rect.Y + 40, rect.Width - 32, rect.Height - 52,
            HorizontalAlignment.Left, VerticalAlignment.Top);
    }

    private string GetTitle() => _state.Champion switch
    {
        Champion.Charioteer => "THE LOOPING ARENA",
        Champion.SpacePilot => "THE ORBITAL LOOP",
        _ => "THE LOOPING SPEEDWAY"
    };

    private static string GetLocationName(Champion? champion) => champion switch
    {
        Champion.Charioteer => "The Looping Arena",
        Champion.SpacePilot => "The Orbital Loop",
        _ => "The Looping Speedway"
    };

    private static string GetStationName(LoopStation station) => station switch
    {
        LoopStation.Input => "INPUT",
        LoopStation.Update => "UPDATE",
        LoopStation.Render => "RENDER",
        LoopStation.Wait => "WAIT",
        _ => "LOOP"
    };

    private static string GetStationDescription(LoopStation station) => station switch
    {
        LoopStation.Input => "Read what the player wants to do. Controller, keyboard, touch — later, even SignalR.",
        LoopStation.Update => "Advance the world. Move objects, run game logic, animate and check collisions.",
        LoopStation.Render => "Draw the current state of the world so the player can see what changed.",
        LoopStation.Wait => "Yield until the next frame, then go around the loop again.",
        _ => string.Empty
    };

    private static (string Title, string Body, string Why) GetOrbitExplanation(LoopStation station) => station switch
    {
        LoopStation.Input => (
            "INPUT",
            "Read controller / touch / network input.",
            "Input changes intent; the game world still changes inside Update."),
        LoopStation.Update => (
            "ORBIT UPDATE",
            "public override void Update(double elapsed)\n{\n    position += speed * elapsed;\n}",
            "Using elapsed time keeps motion consistent even when frame rate changes."),
        LoopStation.Render => (
            "ORBIT RENDER",
            "public override void Render(\n    ICanvas canvas, RectF bounds)\n{\n    DrawVehicle(canvas);\n}",
            "Render describes the current world. It does not decide how time advances."),
        LoopStation.Wait => (
            "FRAME PACING",
            "Orbit yields until the next frame.",
            "The loop repeats continuously without needing a button click to wake it up."),
        _ => ("GAME LOOP", string.Empty, string.Empty)
    };
}
