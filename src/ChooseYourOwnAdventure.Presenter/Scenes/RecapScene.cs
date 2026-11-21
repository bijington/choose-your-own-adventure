using ChooseYourOwnAdventure.Presentation;
using ChooseYourOwnAdventure.Live;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class RecapScene : PresentationSceneBase, IDisposable
{
    private readonly AdventureState _state;
    private readonly TalkSummaryOptions _summary;
    private readonly QrCodeRenderer? _resourcesQr;

    public RecapScene(AdventureState state, TalkSummaryOptions summary)
    {
        _state = state;
        _summary = summary;
        if (summary.ResourcesUri is { } uri)
            _resourcesQr = QrCodeRenderer.ForUrl(uri.AbsoluteUri);
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "THANK YOU FOR CHOOSING\nYOUR OWN ADVENTURE",
            "One MAUI app. A game loop. An entire room playing together.");

        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .028f, 25f, 40f);
        canvas.DrawString(_summary.SpeakerName, left.X, left.Y, left.Width, dimensions.Height * .065f,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * .015f, 14f, 22f);
        canvas.DrawString("Building interactive games with .NET MAUI", left.X,
            left.Y + dimensions.Height * .075f, left.Width, dimensions.Height * .07f,
            HorizontalAlignment.Left, VerticalAlignment.Top);

        var top = dimensions.Y + dimensions.Height * .43f;
        var rowHeight = dimensions.Height * .115f;
        var gap = dimensions.Height * .018f;
        DrawSummary(canvas, new RectF(left.X, top, left.Width, rowHeight),
            "START WITH MAUI", "An app, a canvas and a world to draw.");
        DrawSummary(canvas, new RectF(left.X, top + rowHeight + gap, left.Width, rowHeight),
            "BRING IT TO LIFE WITH ORBIT", "Scenes · GameObjects · Update + Render · input");
        DrawSummary(canvas, new RectF(left.X, top + 2 * (rowHeight + gap), left.Width, rowHeight),
            "LET THE ROOM PLAY WITH SIGNALR", "Audience intent → shared state → live feedback");

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * .021f, 20f, 30f);
        canvas.DrawString("TAKE THE NEXT STEP", right.X, right.Y, right.Width, dimensions.Height * .06f,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * .015f, 14f, 22f);
        canvas.DrawString("The blog post, code and resources\nfrom this adventure.", right.X,
            right.Y + dimensions.Height * .075f, right.Width, dimensions.Height * .075f,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        var size = Math.Min(right.Width * .66f, dimensions.Height * .29f);
        var qr = new RectF(right.X + (right.Width - size) / 2f,
            dimensions.Y + dimensions.Height * .425f, size, size);
        if (_resourcesQr is not null)
        {
            _resourcesQr.Draw(canvas, qr);
        }
        else
        {
            canvas.FillColor = Panel;
            canvas.FillRoundedRectangle(qr, 18);
            canvas.StrokeColor = Border;
            canvas.StrokeSize = 2;
            canvas.DrawRoundedRectangle(qr, 18);
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontColor = Muted;
            canvas.FontSize = Math.Clamp(size * .075f, 15f, 22f);
            canvas.DrawString("RESOURCES\nCOMING SOON", qr, HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(dimensions.Width * .014f, 13f, 20f);
        canvas.DrawString(_resourcesQr is null ? "Blog post link to follow." : _summary.ResourcesUri!.AbsoluteUri,
            right.X, dimensions.Y + dimensions.Height * .74f, right.Width, dimensions.Height * .08f,
            HorizontalAlignment.Center, VerticalAlignment.Top);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * .014f, 14f, 21f);
        canvas.DrawString("The input source changed. The game loop stayed the same.",
            left.X, dimensions.Y + dimensions.Height * .85f, left.Width, dimensions.Height * .07f,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * .011f, 12f, 17f);
        canvas.DrawString("AI was used in the making of these slides,\nbut no agents were harmed in the process.",
            right.X, dimensions.Y + dimensions.Height * .85f, right.Width, dimensions.Height * .07f,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.Recap, _state.Champion));
    }

    private static void DrawSummary(ICanvas canvas, RectF rect, string title, string body)
    {
        canvas.FillColor = Panel;
        canvas.FillRoundedRectangle(rect, 14);
        canvas.StrokeColor = Border;
        canvas.StrokeSize = 1.5f;
        canvas.DrawRoundedRectangle(rect, 14);
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(rect.Width * .036f, 14f, 20f);
        canvas.DrawString(title, rect.X + 18, rect.Y + 8, rect.Width - 36, rect.Height * .37f,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(rect.Width * .033f, 13f, 18f);
        canvas.DrawString(body, rect.X + 18, rect.Y + rect.Height * .48f, rect.Width - 36, rect.Height * .44f,
            HorizontalAlignment.Left, VerticalAlignment.Top);
    }

    public void Dispose() => _resourcesQr?.Dispose();
}
