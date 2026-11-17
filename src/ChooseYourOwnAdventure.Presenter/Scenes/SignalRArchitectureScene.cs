using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class SignalRArchitectureScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    public SignalRArchitectureScene(AdventureState state) => _state = state;

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        var stage = _state.SignalRStage;
        var step = SignalRWalkthrough.ForStage(stage);
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "ONE VOTE, FOUR STEPS",
            $"{(int)stage + 1} / 4 · {step.Title}\nThe champion vote has already made this journey.");

        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        DrawCodeCard(canvas, new RectF(left.X, left.Y, left.Width, left.Height * .84f),
            step.CodeTitle, step.Code);

        // Keep the full route visible. The active component tracks the code on the left.
        var stages = Enum.GetValues<SignalRStage>();
        var boxHeight = dimensions.Height * .095f;
        var gap = dimensions.Height * .035f;
        for (var i = 0; i < stages.Length; i++)
        {
            var rect = new RectF(right.X, right.Y + i * (boxHeight + gap), right.Width, boxHeight);
            DrawPill(canvas, rect, $"{i + 1}   {SignalRWalkthrough.ForStage(stages[i]).Component}", stages[i] == stage);
            if (i == stages.Length - 1) continue;
            canvas.StrokeColor = i == (int)stage ? Accent : Muted;
            canvas.StrokeSize = 2;
            var x = rect.Center.X;
            var top = rect.Bottom + gap * .18f;
            var bottom = rect.Bottom + gap * .82f;
            canvas.DrawLine(x, top, x, bottom);
            canvas.DrawLine(x - 5, bottom - 5, x, bottom);
            canvas.DrawLine(x + 5, bottom - 5, x, bottom);
        }

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * .014f, 14f, 21f);
        canvas.DrawString(step.Caption, left.X, left.Y + left.Height * .87f,
            left.Width, left.Height * .13f, HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * .014f, 14f, 21f);
        canvas.DrawString("Browser sends intent.\nServer owns shared state.\nOrbit renders the result.",
            right.X, dimensions.Y + dimensions.Height * .77f, right.Width, dimensions.Height * .09f,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        DrawPrompt(canvas, dimensions, stage == SignalRStage.Presenter
            ? "SOUTH / RB / ENTER / PGDN · THE AUDIENCE RACE"
            : "SOUTH / RB / ENTER / PGDN · NEXT STEP");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.SignalRArchitecture, _state.Champion));
    }
}
