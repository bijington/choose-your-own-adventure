using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class GameSceneViewIntroScene : PresentationSceneBase
{
    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "GIVE ORBIT SOMEWHERE TO RENDER",
            "GameSceneView is still just a MAUI view sitting inside a MAUI page.");

        var rect = BookLayout.RightPage(dimensions);
        var left = BookLayout.LeftPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .018f, 17f, 27f);
        canvas.DrawString("A place for the world to appear.\n\nGameSceneView is a MAUI view. Place it in your page, just like any other control.\n\nOrbit draws the current scene inside it.", left.X, left.Y + 20, left.Width, left.Height,
            HorizontalAlignment.Left, VerticalAlignment.Top);
        DrawCodeCard(canvas, rect, "MainPage.xaml",
            "<orbit:GameSceneView\n    x:Name=\"GameView\"\n    HorizontalOptions=\"Fill\"\n    VerticalOptions=\"Fill\" />");

        DrawPrompt(canvas, dimensions);
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.GameSceneView));
    }
}
