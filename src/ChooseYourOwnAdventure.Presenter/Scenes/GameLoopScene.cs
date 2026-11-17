using ChooseYourOwnAdventure.GameObjects;
using ChooseYourOwnAdventure.Presentation;
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
        PresentationTheme.DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.GameLoop, _state.Champion));
    }

    private static void DrawBackground(ICanvas canvas, RectF dimensions)
    {
        PresentationTheme.DrawParchment(canvas, dimensions);
    }

    private void DrawHeading(ICanvas canvas, RectF dimensions)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.031f, 24f, 46f);
        canvas.DrawString(GetTitle(), dimensions.X + 40, dimensions.Y + dimensions.Height * 0.05f,
            Math.Max(0, dimensions.Width - 80), 52, HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.MutedInk;
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
                canvas.StrokeColor = PresentationTheme.CodePanelBorder;
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

            canvas.FillColor = active ? Color.FromArgb("#FFF0C2") : PresentationTheme.CodePanel;
            canvas.FillEllipse(position.X - radius, position.Y - radius, radius * 2, radius * 2);
            canvas.StrokeColor = Color.FromArgb("#F8D866");
            canvas.StrokeSize = active ? 4 : 2;
            canvas.DrawEllipse(position.X - radius, position.Y - radius, radius * 2, radius * 2);

            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontColor = active ? PresentationTheme.AccentRed : PresentationTheme.Ink;
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
        canvas.FillColor = PresentationTheme.CodePanel;
        canvas.FillRoundedRectangle(rect, 14);
        canvas.StrokeColor = PresentationTheme.CodePanelBorder;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 14);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.AccentRed;
        canvas.FontSize = Math.Clamp(rect.Width * 0.040f, 15f, 22f);
        canvas.DrawString(title, rect.X + 16, rect.Y + 2, rect.Width - 32, 44,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = CodeFonts.Regular;
        canvas.FontColor = PresentationTheme.CodeText;
        canvas.FontSize = Math.Clamp(rect.Width * 0.031f, 13f, 18f);
        canvas.DrawString(code, rect.X + 16, rect.Y + 50, rect.Width - 32, rect.Height - 62,
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
        canvas.FillColor = PresentationTheme.CodePanel;
        canvas.FillRoundedRectangle(rect, 14);
        canvas.StrokeColor = PresentationTheme.CodePanelBorder;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 14);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.AccentRed;
        canvas.FontSize = Math.Clamp(rect.Width * 0.040f, 15f, 22f);
        canvas.DrawString(title, rect.X + 16, rect.Y + 2, rect.Width - 32, 44,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = PresentationTheme.CodeText;
        canvas.FontSize = Math.Clamp(rect.Width * 0.032f, 13f, 18f);
        canvas.DrawString(body, rect.X + 16, rect.Y + 50, rect.Width - 32, rect.Height - 62,
            HorizontalAlignment.Left, VerticalAlignment.Top);
    }

    private void DrawPrompt(ICanvas canvas, RectF dimensions)
    {
        var bottom = dimensions.Bottom - 94f;

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = PresentationTheme.MutedInk;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.011f, 11f, 16f);
        canvas.DrawString("TAP / SOUTH / ENTER · MOVE TO NEXT NODE",
            dimensions.X + 40, bottom, dimensions.Width * 0.62f, 34,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        var nextWidth = Math.Clamp(dimensions.Width * 0.20f, 150f, 240f);
        var next = new RectF(
            dimensions.Right - nextWidth - 40,
            bottom - 2,
            nextWidth,
            38);

        canvas.FillColor = Color.FromArgb("#FFF4D8");
        canvas.FillRoundedRectangle(next, 12);
        canvas.StrokeColor = PresentationTheme.AccentRed;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(next, 12);

        canvas.FontColor = PresentationTheme.AccentRed;
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
