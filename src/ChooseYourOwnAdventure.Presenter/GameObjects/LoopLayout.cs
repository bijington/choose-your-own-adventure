using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.GameObjects;

public static class LoopLayout
{
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

    public static PointF GetStationPosition(RectF track, LoopStation station) => station switch
    {
        LoopStation.Input => new PointF(track.X + track.Width / 2f, track.Y),
        LoopStation.Update => new PointF(track.Right, track.Y + track.Height / 2f),
        LoopStation.Render => new PointF(track.X + track.Width / 2f, track.Bottom),
        LoopStation.Wait => new PointF(track.X, track.Y + track.Height / 2f),
        _ => new PointF(track.X + track.Width / 2f, track.Y)
    };
}
