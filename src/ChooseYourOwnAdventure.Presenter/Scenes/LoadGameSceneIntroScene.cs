using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class LoadGameSceneIntroScene : PresentationSceneBase
{
    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "LOAD A GAME SCENE",
            "Register Orbit → give it a view → load a scene. Start brings the world to life.");

        var rect = BookLayout.RightPage(dimensions);
        var left = BookLayout.LeftPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .018f, 17f, 27f);
        canvas.DrawString("Bring the world to life.\n\nLoadScene selects the scene to display.\n\nStart begins the loop: update, render, repeat.", left.X, left.Y + 20, left.Width, left.Height,
            HorizontalAlignment.Left, VerticalAlignment.Top);
        DrawCodeCard(canvas, rect, "MainPage.xaml.cs",
            "gameSceneManager\n    .LoadScene<OpeningScene>(GameView);\n\ngameSceneManager.Start();");

        DrawPrompt(canvas, dimensions, "SOUTH / RB / ENTER / PGDN · WHAT DOES START BEGIN?");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.LoadGameScene));
    }
}
