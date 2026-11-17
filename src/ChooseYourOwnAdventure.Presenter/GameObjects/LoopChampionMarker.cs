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

        ChampionSprites.DrawCharacter(canvas, _state.Champion ?? Champion.RacingDriver,
            new RectF(-24f, -40f, 48f, 64f));

        canvas.RestoreState();
    }
}
