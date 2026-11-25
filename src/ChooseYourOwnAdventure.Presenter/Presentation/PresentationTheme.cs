using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Presentation;

public static class PresentationTheme
{
    public static readonly Color Parchment = Color.FromArgb("#F5E8C8");
    public static readonly Color ParchmentLight = Color.FromArgb("#FFF7E3");
    public static readonly Color Ink = Color.FromArgb("#342116");
    public static readonly Color MutedInk = Color.FromArgb("#7A6654");
    public static readonly Color Rule = Color.FromArgb("#A97835");
    public static readonly Color AccentRed = Color.FromArgb("#D84A1B");
    public static readonly Color AccentGold = Color.FromArgb("#E4B83E");
    public static readonly Color DarkPanel = Color.FromArgb("#38251C");
    public static readonly Color DarkPanelBorder = Color.FromArgb("#7A6654");

    // Code is styled like an inset printed page rather than a dark IDE panel.
    public static readonly Color CodePanel = Color.FromArgb("#F8EDCF");
    public static readonly Color CodePanelHeader = Color.FromArgb("#F1DCA8");
    public static readonly Color CodePanelBorder = Color.FromArgb("#B9833F");
    public static readonly Color CodeText = Color.FromArgb("#3B271C");
    public static readonly Color CodeMuted = Color.FromArgb("#866E59");
    public static readonly Color CodeActive = Color.FromArgb("#B63C18");
    public const float InactiveCodeOpacity = .32f;

    // Courses sit behind the sprite rather than competing with its dark outline.
    public static readonly Color TrackSurface = Color.FromArgb("#E5D5AF");
    public static readonly Color TrackEdge = Color.FromArgb("#B49A6F");
    public static readonly Color TrackGuide = Color.FromArgb("#C9B48A");

    public static readonly Color CodeMetric = Color.FromArgb("#9A6816");



    public static void DrawPageNumber(ICanvas canvas, RectF dimensions, int pageNumber, string label = "PAGE")
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = MutedInk;
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.012f, 13f, 18f);
        canvas.DrawString(
            $"— {pageNumber} —",
            dimensions.Right - 170f,
            dimensions.Bottom - 47f,
            125f,
            24f,
            HorizontalAlignment.Right,
            VerticalAlignment.Center);

    }

    public static int GetPageNumber(PresentationSlide slide, Champion? champion = null)
        => slide switch
        {
            PresentationSlide.Title => 1,
            PresentationSlide.AfterParty => 404,
            PresentationSlide.AfterCreditsRace => 405,
            _ => PresentationDeck.StoryPageNumber(slide)
                + (PresentationDeck.UsesChampionOffset(slide) ? (int)(champion ?? 0) : 0)
        };

    public static void DrawParchment(ICanvas canvas, RectF dimensions)
    {
        // Leather cover, layered paper edges, and two facing leaves.
        canvas.FillColor = Color.FromArgb("#38251C");
        canvas.FillRectangle(dimensions);
        for (var i = 0; i < 4; i++)
        {
            canvas.FillColor = i % 2 == 0 ? Color.FromArgb("#CDBB94") : Parchment;
            canvas.FillRoundedRectangle(new RectF(dimensions.X + 18 + i * 3,
                dimensions.Y + 22 - i * 2, Math.Max(0, dimensions.Width - 36 - i * 6),
                Math.Max(0, dimensions.Height - 38 - i * 2)), 5);
        }

        var centre = dimensions.X + dimensions.Width / 2;
        var top = dimensions.Y + 18;
        var height = Math.Max(0, dimensions.Height - 48);
        canvas.FillColor = ParchmentLight;
        canvas.FillRectangle(dimensions.X + 30, top, Math.Max(0, dimensions.Width / 2 - 30), height);
        canvas.FillColor = Color.FromArgb("#FCF1D7");
        canvas.FillRectangle(centre, top, Math.Max(0, dimensions.Width / 2 - 30), height);

        canvas.SaveState();
        for (var i = 0; i < 28; i++)
        {
            canvas.Alpha = .16f * (1 - i / 28f) * (1 - i / 28f);
            canvas.FillColor = Ink;
            canvas.FillRectangle(centre - i - 1, top, 1, height);
            canvas.FillRectangle(centre + i, top, 1, height);
        }
        canvas.RestoreState();
        canvas.StrokeColor = Color.FromArgb("#B99A68");
        canvas.StrokeSize = 1;
        canvas.DrawLine(centre, top, centre, top + height);
    }
}
