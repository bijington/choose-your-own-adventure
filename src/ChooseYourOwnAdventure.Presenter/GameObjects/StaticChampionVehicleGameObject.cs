using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.GameObjects;

public sealed class StaticChampionVehicleGameObject : ChampionVehicleGameObject
{
    public Champion Champion { get; set; }
    public PointF Position { get; set; }
    public float Scale { get; set; } = 1f;

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        base.Render(canvas, dimensions);
        canvas.SaveState();
        canvas.Translate(Position.X, Position.Y);
        canvas.Scale(Scale, Scale);
        ChampionSprites.DrawSideVehicle(canvas, Champion, new RectF(-38f, -26f, 76f, 52f));
        canvas.RestoreState();
    }
}
