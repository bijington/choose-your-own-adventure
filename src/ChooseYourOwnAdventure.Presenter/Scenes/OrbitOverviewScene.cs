using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class OrbitOverviewScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    public OrbitOverviewScene(AdventureState state) => _state = state;

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        var stage = _state.OrbitOverviewStage;
        var step = OrbitOverviewWalkthrough.ForStage(stage);
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "WHAT IS ORBIT?",
            $"{(int)stage + 1} / 4 · {step.Component}\nHow do we build a game inside a MAUI app?");

        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * .023f, 22f, 34f);
        canvas.DrawString(step.Title, left.X, left.Y, left.Width, left.Height * .16f,
            HorizontalAlignment.Left, VerticalAlignment.Top);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .018f, 17f, 27f);
        canvas.DrawString(step.Explanation, left.X, left.Y + left.Height * .20f,
            left.Width, left.Height * .60f, HorizontalAlignment.Left, VerticalAlignment.Top);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * .016f, 16f, 24f);
        canvas.DrawString(step.Takeaway, left.X, left.Y + left.Height * .85f,
            left.Width, left.Height * .15f, HorizontalAlignment.Left, VerticalAlignment.Center);

        var stages = Enum.GetValues<OrbitOverviewStage>();
        var height = dimensions.Height * .095f;
        var gap = dimensions.Height * .035f;
        for (var i = 0; i < stages.Length; i++)
        {
            var rect = new RectF(right.X, right.Y + i * (height + gap), right.Width, height);
            DrawPill(canvas, rect, $"{i + 1}   {OrbitOverviewWalkthrough.ForStage(stages[i]).Component}", stages[i] == stage);
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

        DrawPrompt(canvas, dimensions, stage == OrbitOverviewStage.Game
            ? "SOUTH / RB / ENTER / PGDN · ADD ORBIT TO THE APP"
            : "SOUTH / RB / ENTER / PGDN · NEXT BUILDING BLOCK");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.OrbitOverview));
    }
}
