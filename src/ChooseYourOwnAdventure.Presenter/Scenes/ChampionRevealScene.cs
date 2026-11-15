using ChooseYourOwnAdventure.Presentation;
using ChooseYourOwnAdventure.GameObjects;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class ChampionRevealScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    public ChampionRevealScene(AdventureState state)
    {
        _state = state;
        Add(new ChampionGameObject(state, onBookPage: true));
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "OUR MAIN CHARACTER",
            "That object is being rendered by the real Orbit GameObject we just added.");

        var pageNumber = PresentationTheme.GetPageNumber(PresentationSlide.ChampionReveal, _state.Champion);
        var left = BookLayout.LeftPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .019f, 18f, 28f);
        canvas.DrawString("Our first GameObject.\n\nThe scene decides where it belongs.\n\nThe object decides how it renders.\n\nNow… how do we make it move?", left.X, left.Y + 20, left.Width, left.Height,
            HorizontalAlignment.Left, VerticalAlignment.Top);

        base.Render(canvas, dimensions);

        DrawPrompt(canvas, dimensions);
        DrawPageNumber(canvas, dimensions, pageNumber);
    }
}
