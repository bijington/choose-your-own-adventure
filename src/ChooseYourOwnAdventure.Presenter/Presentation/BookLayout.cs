using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Presentation;

public static class BookLayout
{
    public static RectF LeftPage(RectF bounds) => new(bounds.X + bounds.Width * .065f,
        bounds.Y + bounds.Height * .25f, bounds.Width * .395f, bounds.Height * .58f);

    public static RectF RightPage(RectF bounds) => new(bounds.X + bounds.Width * .54f,
        bounds.Y + bounds.Height * .25f, bounds.Width * .395f, bounds.Height * .58f);
}
