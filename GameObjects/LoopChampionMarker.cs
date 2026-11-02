using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.GameObjects;

/// <summary>
/// Represents the chosen champion walking around the conceptual game loop.
/// One presentation action moves the champion to the next station.
/// </summary>
public sealed class LoopChampionMarker : GameObject
{
    private readonly AdventureState _state;

    public LoopChampionMarker(AdventureState state)
    {
        _state = state;
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        base.Render(canvas, dimensions);

        var track = LoopLayout.GetTrackBounds(dimensions);
        var position = LoopLayout.GetStationPosition(track, _state.GameLoopStation);

        canvas.SaveState();
        canvas.Translate(position.X, position.Y - 42f);

        switch (_state.Champion)
        {
            case Champion.Charioteer:
                DrawCharioteer(canvas);
                break;
            case Champion.SpacePilot:
                DrawSpacePilot(canvas);
                break;
            default:
                DrawRacingDriver(canvas);
                break;
        }

        canvas.RestoreState();
    }

    private static void DrawRacingDriver(ICanvas canvas)
    {
        canvas.FillColor = Color.FromArgb("#EF4444");
        canvas.FillRoundedRectangle(new RectF(-18, -18, 36, 30), 8);
        canvas.FillColor = Color.FromArgb("#F8FAFC");
        canvas.FillRoundedRectangle(new RectF(-12, -30, 24, 17), 7);
        canvas.FillColor = Color.FromArgb("#38BDF8");
        canvas.FillRoundedRectangle(new RectF(-8, -26, 16, 9), 4);
        DrawFeet(canvas);
    }

    private static void DrawCharioteer(ICanvas canvas)
    {
        canvas.FillColor = Color.FromArgb("#D97706");
        canvas.FillRoundedRectangle(new RectF(-18, -17, 36, 29), 7);
        canvas.FillColor = Color.FromArgb("#F1C7A5");
        canvas.FillEllipse(-8, -33, 16, 16);
        canvas.FillColor = Color.FromArgb("#F8D866");
        canvas.FillRectangle(-11, -37, 22, 6);
        DrawFeet(canvas);
    }

    private static void DrawSpacePilot(ICanvas canvas)
    {
        canvas.FillColor = Color.FromArgb("#6366F1");
        canvas.FillRoundedRectangle(new RectF(-18, -17, 36, 29), 8);
        canvas.FillColor = Color.FromArgb("#CBD5E1");
        canvas.FillRoundedRectangle(new RectF(-13, -34, 26, 20), 9);
        canvas.FillColor = Color.FromArgb("#22D3EE");
        canvas.FillRoundedRectangle(new RectF(-9, -29, 18, 9), 4);
        DrawFeet(canvas);
    }

    private static void DrawFeet(ICanvas canvas)
    {
        canvas.FillColor = Color.FromArgb("#111827");
        canvas.FillRectangle(-14, 10, 10, 8);
        canvas.FillRectangle(4, 10, 10, 8);
    }
}
