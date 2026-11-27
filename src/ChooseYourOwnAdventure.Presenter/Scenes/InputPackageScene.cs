using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class InputPackageScene(AdventureState state) : PresentationSceneBase
{
    public override void Render(ICanvas canvas, RectF dimensions)
    {
        var stage = state.ControllerStage;
        var step = ControllerWalkthrough.ForStage(stage);
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "CONNECT A GAME CONTROLLER",
            $"{(int)stage + 1} / 4 · {step.Title}\nFollow the real input bridge used by this presentation.");
        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        DrawCodeCard(canvas, new RectF(left.X, left.Y, left.Width, left.Height * .96f), step.CodeTitle, step.Code);
        var stages = Enum.GetValues<ControllerStage>();
        var labels = new[] { "REGISTER", "DISCOVER", "CONNECT + SUBSCRIBE", "READ + MAP TO INTENT" };
        for (var i = 0; i < stages.Length; i++)
            DrawPill(canvas, new RectF(right.X, right.Y + right.Height * i * .145f, right.Width, right.Height * .11f),
                $"{i + 1}   {labels[i]}", stages[i] == stage);
        canvas.FontColor = Ink;
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontSize = Math.Clamp(dimensions.Width * .015f, 15, 23);
        canvas.DrawString(step.Explanation, right.X, right.Y + right.Height * .64f, right.Width, right.Height * .35f,
            HorizontalAlignment.Left, VerticalAlignment.Top);
        DrawPrompt(canvas, dimensions, stage == ControllerStage.Actions
            ? "SOUTH / RB / ENTER / PGDN · PUT OUR CHAMPION IN MOTION"
            : "SOUTH / RB / ENTER / PGDN · NEXT CONTROLLER STEP");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.InputPackage, state.Champion));
    }
}
