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
    public static readonly Color DarkPanel = Color.FromArgb("#111827");
    public static readonly Color DarkPanelBorder = Color.FromArgb("#4B5563");

    // Code is styled like an inset printed page rather than a dark IDE panel.
    public static readonly Color CodePanel = Color.FromArgb("#F8EDCF");
    public static readonly Color CodePanelHeader = Color.FromArgb("#F1DCA8");
    public static readonly Color CodePanelBorder = Color.FromArgb("#B9833F");
    public static readonly Color CodeText = Color.FromArgb("#3B271C");
    public static readonly Color CodeMuted = Color.FromArgb("#866E59");
    public static readonly Color CodeActive = Color.FromArgb("#B63C18");
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
        => (int)slide + (slide >= PresentationSlide.BranchResult ? (int)(champion ?? 0) : 0);

    public static void DrawParchment(ICanvas canvas, RectF dimensions)
    {
        canvas.FillColor = Parchment;
        canvas.FillRectangle(dimensions);

        // Warm inner page, surrounded by a subtle darker edge.
        var inset = 24f;
        var inner = new RectF(
            dimensions.X + inset,
            dimensions.Y + inset,
            Math.Max(0, dimensions.Width - inset * 2),
            Math.Max(0, dimensions.Height - inset * 2));

        canvas.FillColor = ParchmentLight;
        canvas.FillRoundedRectangle(inner, 14f);
        canvas.StrokeColor = Rule;
        canvas.StrokeSize = 2f;
        canvas.DrawRoundedRectangle(inner, 14f);

        // Quiet book-page rules in the corners give the parchment some texture
        // without compromising projector readability.
        canvas.Alpha = 0.18f;
        canvas.StrokeColor = Rule;
        canvas.StrokeSize = 1f;
        var step = Math.Max(72f, dimensions.Width / 18f);
        for (var x = inner.X + step; x < inner.Right - step; x += step)
        {
            canvas.DrawLine(x, inner.Y + 8, x + 16, inner.Y + 8);
            canvas.DrawLine(x, inner.Bottom - 8, x + 16, inner.Bottom - 8);
        }
        canvas.Alpha = 1f;
    }
}
