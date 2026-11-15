using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Presentation;

public static class ChampionSelectionLayout
{
    public static RectF Card(RectF bounds, Champion champion)
    {
        var page = BookLayout.RightPage(bounds);
        var row = champion switch
        {
            Champion.Charioteer => 1,
            Champion.SpacePilot => 2,
            _ => 0
        };
        return new RectF(page.X, bounds.Y + bounds.Height * (.22f + row * .22f),
            page.Width, bounds.Height * .195f);
    }

    public static Champion? HitTest(RectF bounds, PointF point)
    {
        foreach (var champion in Enum.GetValues<Champion>())
            if (Card(bounds, champion).Contains(point)) return champion;
        return null;
    }
}
