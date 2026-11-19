using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class MauiInputScene : PresentationSceneBase
{
    private readonly AdventureState _state;

    public MauiInputScene(AdventureState state) => _state = state;

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "MAUI ALREADY HAS INPUT",
            "A touch screen or a mouse can control our champion through ordinary MAUI UI.");
        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        DrawCodeCard(canvas, new RectF(left.X, left.Y, left.Width, left.Height * .55f),
            "XAML · a held control", """
<Button
    Text="Hold to move"
    Pressed="OnMovePressed"
    Released="OnMoveReleased" />
""");
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * .017f, 17f, 25f);
        canvas.DrawString("TOUCH OR CLICK. SAME INTENT.", left.X, left.Y + left.Height * .61f,
            left.Width, left.Height * .11f, HorizontalAlignment.Left, VerticalAlignment.Center);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .015f, 15f, 22f);
        canvas.DrawString("Buttons and commands for actions.\nPressed + Released for held input.\nGestures and pointer events for richer interaction.",
            left.X, left.Y + left.Height * .75f, left.Width, left.Height * .25f,
            HorizontalAlignment.Left, VerticalAlignment.Top);

        DrawCodeCard(canvas, new RectF(right.X, right.Y, right.Width, right.Height * .85f),
            "C# · feed the input router", """
void OnMovePressed(
    object? sender, EventArgs e) =>
    _inputRouter.SetPressed(
        AdventureAction.Confirm, true);

void OnMoveReleased(
    object? sender, EventArgs e) =>
    _inputRouter.SetPressed(
        AdventureAction.Confirm, false);
""");
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * .014f, 14f, 21f);
        canvas.DrawString("UI sets intent. Update moves. Render draws.",
            right.X, right.Y + right.Height * .89f, right.Width, right.Height * .11f,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        DrawPrompt(canvas, dimensions, "SOUTH / ENTER · WHAT ABOUT KEYBOARDS AND CONTROLLERS?");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.MauiInput, _state.Champion));
    }
}
