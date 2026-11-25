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
        DrawTitle(canvas, dimensions, "THE END",
            "An adventure made by the people in this room.");

        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        DrawCredit(canvas, left, .00f, "STARRING", "You, the audience",
            "The Driver · The Charioteer · The Space Pilot");
        DrawCredit(canvas, left, .23f, "WRITTEN & PRESENTED BY", _summary.SpeakerName,
            "With several decisions delegated to the room");
        DrawCredit(canvas, left, .46f, "BUILT WITH", ".NET MAUI · MAUI Graphics",
            "Orbit · SignalR");
        DrawCredit(canvas, left, .69f, "THE STORY", "Intent → state → a world brought to life",
            "You chose the champions. You powered the ending.");

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(dimensions.Width * .021f, 20f, 30f);
        canvas.DrawString("CONTINUE THE ADVENTURE", right.X, right.Y, right.Width, dimensions.Height * .06f,
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

    private static void DrawCredit(ICanvas canvas, RectF page, float position,
        string role, string name, string detail)
    {
        var y = page.Y + page.Height * position;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(page.Width * .029f, 12f, 17f);
        canvas.DrawString(role, page.X, y, page.Width, page.Height * .05f,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.FontColor = Ink;
        canvas.FontSize = Math.Clamp(page.Width * .044f, 16f, 26f);
        canvas.DrawString(name, page.X, y + page.Height * .065f, page.Width, page.Height * .065f,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(page.Width * .029f, 12f, 17f);
        canvas.DrawString(detail, page.X, y + page.Height * .14f, page.Width, page.Height * .065f,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    public void Dispose() => _resourcesQr?.Dispose();
}
