using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class LoadGameSceneIntroScene : PresentationSceneBase
{
    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "LOAD A GAME SCENE",
            "Register Orbit → give it a view → load a scene. Now our adventure needs a main character…");

        var rect = new RectF(dimensions.X + dimensions.Width * 0.12f,
            dimensions.Y + dimensions.Height * 0.28f,
            dimensions.Width * 0.76f, dimensions.Height * 0.38f);
        DrawCodeCard(canvas, rect, "MainPage.xaml.cs",
            "gameSceneManager.LoadScene<OpeningScene>(GameView);\n\ngameSceneManager.Start();");

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.022f, 19f, 32f);
        canvas.DrawString("Every good adventure needs a main character…", dimensions.X + 60,
            dimensions.Y + dimensions.Height * 0.75f, dimensions.Width - 120, 42,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        DrawPrompt(canvas, dimensions);
    }
}
