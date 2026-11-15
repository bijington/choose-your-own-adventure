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

        var rect = new RectF(dimensions.X + dimensions.Width * 0.13f,
            dimensions.Y + dimensions.Height * 0.28f,
            dimensions.Width * 0.74f, dimensions.Height * 0.42f);
        DrawCodeCard(canvas, rect, "MainPage.xaml",
            "<orbit:GameSceneView\n    x:Name=\"GameView\"\n    HorizontalOptions=\"Fill\"\n    VerticalOptions=\"Fill\" />");

        DrawPrompt(canvas, dimensions);
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.GameSceneView));
    }
}
