using ChooseYourOwnAdventure.GameObjects;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class ChampionRevealScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    public ChampionRevealScene(AdventureState state)
    {
        _state = state;
        Add(new ChampionGameObject(state));
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "OUR MAIN CHARACTER",
            "That object is being rendered by the real Orbit GameObject we just added.");

        base.Render(canvas, dimensions);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.021f, 18f, 30f);
        canvas.DrawString("Now we have a character… how do we make it move?", dimensions.X + 60,
            dimensions.Bottom - 130, dimensions.Width - 120, 42,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        DrawPrompt(canvas, dimensions);
    }
}
