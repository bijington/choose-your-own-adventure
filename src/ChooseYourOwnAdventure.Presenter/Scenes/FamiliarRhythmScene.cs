using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class FamiliarRhythmScene : PresentationSceneBase
{
    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "A FAMILIAR RHYTHM",
            "Start() begins the rhythm: read → update → output → repeat.");
        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        DrawCodeCard(canvas, new RectF(left.X, left.Y, left.Width, left.Height * .86f),
            "FIRMWARE SUPERLOOP", "while (true)\n{\n    ReadInputs();\n    UpdateState();\n    WriteOutputs();\n    Delay();\n}");
        DrawCodeCard(canvas, new RectF(right.X, right.Y, right.Width, right.Height * .86f),
            "GAME LOOP · CONCEPTUALLY", "while (running)\n{\n    Input();\n    Update();\n    Render();\n    Wait();\n}");
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .015f, 15, 23);
        canvas.DrawString("Orbit runs the loop. Our GameObjects supply Update and Render.",
            left.X, dimensions.Y + dimensions.Height * .79f, left.Width, dimensions.Height * .08f,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        DrawPrompt(canvas, dimensions, "SOUTH / RB / ENTER / PGDN · MEET OUR CHAMPION");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.FamiliarRhythm));
    }
}
