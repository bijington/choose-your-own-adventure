namespace ChooseYourOwnAdventure.Presentation;

public sealed record ChampionRenderingStep(string Title, string Caption, string Code, string TalkingPoints);

public static class ChampionRenderingWalkthrough
{
    public static ChampionRenderingStep ForStage(ChampionRenderingStage stage) => stage switch
    {
        ChampionRenderingStage.Dot => new("DOT", "Every champion starts somewhere.", """
canvas.FillColor = PresentationTheme.AccentRed;
canvas.FillEllipse(
    bounds.Center.X - 14,
    bounds.Center.Y - 14, 28, 28);
""", "Start our champion's origin story with a dot. FillEllipse draws a filled circle when width and height match. We still render using a canvas and bounds. Artwork can evolve without changing that contract."),
        ChampionRenderingStage.Primitives => new("PRIMITIVES", "A few shapes. A recognisable character.", """
// Focused excerpt: head, body and legs.
canvas.FillEllipse(x - 16, y - 64, 32, 32);
canvas.FillRectangle(x - 22, y - 25, 44, 54);
canvas.StrokeSize = 10;
canvas.DrawLine(x - 12, y + 29,
    x - 20, y + 70);
canvas.DrawLine(x + 12, y + 29,
    x + 20, y + 70);
""", "Combine circles, rectangles and lines into a little figure. The same drawing API can make recognisable artwork. More hand-drawn detail means more drawing instructions; we can move that visual detail into an asset instead. The figure's colour follows the chosen champion."),
        ChampionRenderingStage.Image => new("IMAGE", "Let the artwork carry the detail.", """
// Decode once; reuse the cached image.
ChampionSprites.DrawCharacter(
    canvas, champion, bounds);

// Inside the aspect-fit helper:
canvas.DrawImage(image, x, y, width, height);
""", "Now show the current champion PNG. The image is decoded once and cached. DrawCharacter preserves its aspect ratio and calls DrawImage. The picture is more detailed, while the code drawing it is simpler than individually describing every shape. This is one static image."),
        ChampionRenderingStage.Sprite => new("ANIMATED SPRITE", "More life. The same rendering contract.", """
// Create once from cached image frames.
_sprite = ChampionSprites.CreateCelebration(
    champion);

// During Update:
_sprite.Update(millisecondsSinceLastUpdate);

// During Render:
_sprite.Bounds = bounds;
_sprite.Render(canvas, dimensions);
""", "Use the real Orbit Sprite and the existing eight celebration frames, at 125 milliseconds per frame. Create the sprite once, advance it from Update, then render it into Bounds. The engine chooses the current frame; we don't redraw each pose by hand. A sprite can be static too; animation is the distinction in this example. The canvas-and-bounds rendering contract stays the same, with animation state advanced separately. This leads directly into the game-loop story."),
        _ => throw new ArgumentOutOfRangeException(nameof(stage))
    };
}
