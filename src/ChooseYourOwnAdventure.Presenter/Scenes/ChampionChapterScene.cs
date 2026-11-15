using ChooseYourOwnAdventure.GameObjects;
using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

/// <summary>A quiet chapter-opening leaf after the audience confirms its champion.</summary>
public sealed class ChampionChapterScene : PresentationSceneBase
{
    private readonly AdventureState _state;
    private readonly ChampionGameObject _champion;

    public ChampionChapterScene(AdventureState state)
    {
        _state = state;
        _champion = new ChampionGameObject(state, chapterPage: true);
        Add(_champion);
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        var right = BookLayout.RightPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * .013f, 13f, 21f);
        canvas.DrawString("CHAPTER ONE", right.X, dimensions.Y + dimensions.Height * .10f,
            right.Width, 42, HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .03f, 26f, 44f);
        canvas.DrawString("MEET YOUR CHAMPION", right.X, dimensions.Y + dimensions.Height * .17f,
            right.Width, dimensions.Height * .12f, HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * .023f, 22f, 34f);
        canvas.DrawString(_champion.ChampionName, right.X, dimensions.Y + dimensions.Height * .31f,
            right.Width, dimensions.Height * .09f, HorizontalAlignment.Center, VerticalAlignment.Center);

        base.Render(canvas, dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * .011f, 12f, 17f);
        canvas.DrawString("A / ENTER · TURN THE PAGE", right.X, dimensions.Bottom - 96,
            right.Width, 42, HorizontalAlignment.Center, VerticalAlignment.Center);
        DrawPageNumber(canvas, dimensions,
            PresentationTheme.GetPageNumber(PresentationSlide.ChampionChapter, _state.Champion));
    }
}
