using ChooseYourOwnAdventure.GameObjects;
using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

/// <summary>A right-hand chapter leaf; the facing page intentionally stays blank.</summary>
public abstract class ChapterOpeningScene : PresentationSceneBase
{
    private readonly AdventureState _state;
    private readonly PresentationSlide _slide;
    private readonly string _section;
    private readonly string _title;
    private readonly string _introduction;
    private readonly ChampionGameObject? _champion;

    protected ChapterOpeningScene(AdventureState state, PresentationSlide slide, string section,
        string title, string introduction, bool showChampion = false)
    {
        _state = state;
        _slide = slide;
        _section = section;
        _title = title;
        _introduction = introduction;
        if (showChampion)
        {
            _champion = new ChampionGameObject(state, chapterPage: true);
            Add(_champion);
        }
    }

    protected virtual string ContinuePrompt => "A / ENTER · TURN THE PAGE";

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        var right = BookLayout.RightPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * .013f, 13f, 21f);
        canvas.DrawString(_section, right.X, dimensions.Y + dimensions.Height * .10f,
            right.Width, 42, HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .03f, 26f, 44f);
        canvas.DrawString(_title, right.X, dimensions.Y + dimensions.Height * .17f,
            right.Width, dimensions.Height * .12f, HorizontalAlignment.Center, VerticalAlignment.Center);

        if (_champion is not null)
        {
            canvas.FontColor = Accent;
            canvas.FontSize = Math.Clamp(dimensions.Width * .023f, 22f, 34f);
            canvas.DrawString(_champion.ChampionName, right.X, dimensions.Y + dimensions.Height * .31f,
                right.Width, dimensions.Height * .08f, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * .017f, 16f, 24f);
        canvas.DrawString(_introduction, right.X + 12,
            dimensions.Y + dimensions.Height * (_champion is null ? .36f : .405f),
            right.Width - 24, dimensions.Height * (_champion is null ? .22f : .14f),
            HorizontalAlignment.Center, VerticalAlignment.Center);

        if (_champion is null)
        {
            var y = dimensions.Y + dimensions.Height * .67f;
            var x = right.X + right.Width / 2;
            canvas.StrokeColor = Gold;
            canvas.StrokeSize = 2;
            canvas.DrawLine(x - right.Width * .16f, y, x - 14, y);
            canvas.DrawLine(x + 14, y, x + right.Width * .16f, y);
            canvas.FillColor = Accent;
            canvas.FillEllipse(x - 4, y - 4, 8, 8);
        }
        base.Render(canvas, dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * .011f, 12f, 17f);
        canvas.DrawString(ContinuePrompt, right.X, dimensions.Bottom - 96,
            right.Width, 42, HorizontalAlignment.Center, VerticalAlignment.Center);
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(_slide, _state.Champion));
    }
}
