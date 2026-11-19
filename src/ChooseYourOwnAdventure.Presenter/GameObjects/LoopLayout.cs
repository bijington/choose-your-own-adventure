using Microsoft.Maui.Graphics;
using ChooseYourOwnAdventure.Presentation;

namespace ChooseYourOwnAdventure.GameObjects;

public static class LoopLayout
{
    public static RectF GetTrackBounds(RectF dimensions)
    {
        var right = BookLayout.RightPage(dimensions);
        return new RectF(right.X + right.Width * .12f,
            dimensions.Y + dimensions.Height * .34f, right.Width * .76f, dimensions.Height * .34f);
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
