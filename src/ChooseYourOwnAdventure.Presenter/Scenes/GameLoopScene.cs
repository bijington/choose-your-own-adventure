using ChooseYourOwnAdventure.GameObjects;
using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class GameLoopScene : GameScene
{
    private readonly AdventureState _state;

    public GameLoopScene(AdventureState state)
    {
        _state = state;
        _state.CurrentLocation = GetLocationName(state.Champion);
        Add(new LoopChampionMarker(state));
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawBackground(canvas, dimensions);
        DrawHeading(canvas, dimensions);
        DrawTrack(canvas, dimensions);
        DrawStations(canvas, dimensions, _state.GameLoopStation);

        if (_state.GameLoopStage == GameLoopStage.FirmwareComparison)
        {
            DrawFirmwareComparison(canvas, dimensions);
        }
        else
        {
            DrawOverview(canvas, dimensions, _state.GameLoopStation);
        }

        base.Render(canvas, dimensions);
        DrawPrompt(canvas, dimensions);
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
        canvas.DrawString(GetTitle(), dimensions.X + 40, dimensions.Y + dimensions.Height * 0.05f,
            Math.Max(0, dimensions.Width - 80), 52, HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.014f, 13f, 21f);
        canvas.DrawString("Walk one iteration of the loop — one button press at a time.", dimensions.X + 40,
            dimensions.Y + dimensions.Height * 0.11f, Math.Max(0, dimensions.Width - 80), 36,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawTrack(ICanvas canvas, RectF dimensions)
    {
        var track = LoopLayout.GetTrackBounds(dimensions);

        switch (_state.Champion)
        {
            case Champion.Charioteer:
                canvas.StrokeColor = Color.FromArgb("#D97706");
                canvas.StrokeSize = 25;
                canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
                canvas.StrokeColor = Color.FromArgb("#F8D866");
                canvas.StrokeSize = 2;
                canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
                break;
            case Champion.SpacePilot:
                canvas.StrokeColor = Color.FromArgb("#312E81");
                canvas.StrokeSize = 28;
                canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
                canvas.StrokeColor = Color.FromArgb("#22D3EE");
                canvas.StrokeSize = 3;
                canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
                break;
            default:
                canvas.StrokeColor = Color.FromArgb("#475569");
                canvas.StrokeSize = 28;
                canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
                canvas.StrokeColor = Color.FromArgb("#CBD5E1");
                canvas.StrokeSize = 2;
                canvas.DrawEllipse(track.X, track.Y, track.Width, track.Height);
                break;
        }
    }

    private static void DrawStations(ICanvas canvas, RectF dimensions, LoopStation activeStation)
    {
        var track = LoopLayout.GetTrackBounds(dimensions);

        foreach (var station in Enum.GetValues<LoopStation>())
        {
            var position = LoopLayout.GetStationPosition(track, station);
            var active = station == activeStation;
            var radius = active ? 31f : 27f;

            if (active)
            {
                canvas.FillColor = Color.FromRgba(248, 216, 102, 72);
                canvas.FillEllipse(position.X - radius - 8, position.Y - radius - 8,
                    (radius + 8) * 2, (radius + 8) * 2);
            }

            canvas.FillColor = Color.FromArgb("#111827");
            canvas.FillEllipse(position.X - radius, position.Y - radius, radius * 2, radius * 2);
            canvas.StrokeColor = Color.FromArgb("#F8D866");
            canvas.StrokeSize = active ? 4 : 2;
            canvas.DrawEllipse(position.X - radius, position.Y - radius, radius * 2, radius * 2);

            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontColor = active ? Color.FromArgb("#F8D866") : Colors.White;
            canvas.FontSize = 13;
            canvas.DrawString(GetStationName(station), position.X - 54, position.Y - 12, 108, 24,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }

    private static void DrawOverview(ICanvas canvas, RectF dimensions, LoopStation station)
    {
        DrawInfoPanel(canvas, dimensions,
            "TYPICAL BUSINESS APP",
            "event → command → state change → UI update → wait",
            $"GAME LOOP · {GetStationName(station)}",
            GetStationDescription(station));
    }

    private static void DrawFirmwareComparison(ICanvas canvas, RectF dimensions)
    {
        DrawCodeComparisonPanel(canvas, dimensions,
            "FIRMWARE SUPERLOOP",
            "while (true)\n{\n    ReadInputs();\n    UpdateState();\n    WriteOutputs();\n    Delay();\n}",
            "GAME LOOP",
            "while (running)\n{\n    Input();\n    Update();\n    Render();\n    Wait();\n}");
    }

    private static void DrawCodeComparisonPanel(ICanvas canvas, RectF dimensions,
        string leftTitle, string leftBody, string rightTitle, string rightBody)
    {
        var top = dimensions.Y + dimensions.Height * 0.735f;
        var width = dimensions.Width * 0.40f;
        var height = dimensions.Height * 0.18f;
        var gap = dimensions.Width * 0.04f;
        var left = new RectF((dimensions.X + dimensions.Width / 2f) - gap / 2f - width, top, width, height);
        var right = new RectF((dimensions.X + dimensions.Width / 2f) + gap / 2f, top, width, height);
        DrawCodeCard(canvas, left, leftTitle, leftBody);
        DrawCodeCard(canvas, right, rightTitle, rightBody);
    }

    private static void DrawCodeCard(ICanvas canvas, RectF rect, string title, string code)
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

        canvas.Font = CodeFonts.Regular;
        canvas.FontColor = Color.FromArgb("#E2E8F0");
        canvas.FontSize = Math.Clamp(rect.Width * 0.027f, 10f, 14f);
        canvas.DrawString(code, rect.X + 16, rect.Y + 40, rect.Width - 32, rect.Height - 52,
            HorizontalAlignment.Left, VerticalAlignment.Top);
    }

    private static void DrawInfoPanel(ICanvas canvas, RectF dimensions,
        string leftTitle, string leftBody, string rightTitle, string rightBody)
    {
        var top = dimensions.Y + dimensions.Height * 0.735f;
        var width = dimensions.Width * 0.40f;
        var height = dimensions.Height * 0.18f;
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

    private void DrawPrompt(ICanvas canvas, RectF dimensions)
    {
        var bottom = dimensions.Bottom - 58f;

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#94A3B8");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.011f, 11f, 16f);
        canvas.DrawString("TAP / A / ENTER · MOVE TO NEXT NODE",
            dimensions.X + 40, bottom, dimensions.Width * 0.62f, 34,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        var nextWidth = Math.Clamp(dimensions.Width * 0.20f, 150f, 240f);
        var next = new RectF(
            dimensions.Right - nextWidth - 40,
            bottom - 2,
            nextWidth,
            38);

        canvas.FillColor = Color.FromArgb("#172033");
        canvas.FillRoundedRectangle(next, 12);
        canvas.StrokeColor = Color.FromArgb("#F8D866");
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(next, 12);

        canvas.FontColor = Color.FromArgb("#F8D866");
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.0105f, 11f, 15f);

        var nextLabel = _state.GameLoopStage == GameLoopStage.Overview
            ? "NEXT →  FIRMWARE"
            : "NEXT →  VEHICLE";

        canvas.DrawString(nextLabel, next.X + 10, next.Y + 4, next.Width - 20, next.Height - 8,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private string GetTitle() => _state.Champion switch
    {
        Champion.Charioteer => "WALK THE LOOPING ARENA",
        Champion.SpacePilot => "WALK THE ORBITAL LOOP",
        _ => "WALK THE LOOPING SPEEDWAY"
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
        LoopStation.Render => "Draw the current world state. Rendering reflects state; it should not decide it.",
        LoopStation.Wait => "Yield until the next frame, then start the cycle again.",
        _ => string.Empty
    };
}
