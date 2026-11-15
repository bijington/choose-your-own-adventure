using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class TitleScene : PresentationSceneBase
{
    private static readonly Lazy<IImage?> TitleImage = new(LoadTitleImage);
    private double _elapsed;

    public override void Update(double millisecondsSinceLastUpdate)
    {
        _elapsed += millisecondsSinceLastUpdate;
        base.Update(millisecondsSinceLastUpdate);
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        canvas.FillColor = Color.FromArgb("#F4E7C6");
        canvas.FillRectangle(dimensions);

        var image = TitleImage.Value;
        if (image is not null)
        {
            DrawAspectFit(canvas, image, dimensions);
        }
        else
        {
            DrawFallback(canvas, dimensions);
        }

        DrawStartPrompt(canvas, dimensions);
    }

    private void DrawStartPrompt(ICanvas canvas, RectF dimensions)
    {
        var pulse = 0.62f + (float)((Math.Sin(_elapsed / 420d) + 1d) * 0.19d);
        var width = Math.Clamp(dimensions.Width * 0.22f, 220f, 360f);
        var height = Math.Clamp(dimensions.Height * 0.052f, 42f, 58f);
        var rect = new RectF(
            dimensions.X + (dimensions.Width - width) / 2f,
            dimensions.Bottom - height - Math.Clamp(dimensions.Height * 0.025f, 16f, 30f),
            width,
            height);

        canvas.Alpha = 0.88f;
        canvas.FillColor = Color.FromArgb("#FFF4D6");
        canvas.FillRoundedRectangle(rect, height / 2f);
        canvas.StrokeColor = Color.FromArgb("#D83A16");
        canvas.StrokeSize = 2f;
        canvas.DrawRoundedRectangle(rect, height / 2f);

        canvas.Alpha = pulse;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#7A2414");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.0135f, 14f, 22f);
        canvas.DrawString(
            "A / ENTER  ·  PRESS START",
            rect.X + 18f,
            rect.Y,
            rect.Width - 36f,
            rect.Height,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);
        canvas.Alpha = 1f;
    }

    private static void DrawAspectFit(ICanvas canvas, IImage image, RectF bounds)
    {
        var imageAspect = image.Width / image.Height;
        var boundsAspect = bounds.Width / bounds.Height;

        float width;
        float height;

        if (boundsAspect > imageAspect)
        {
            height = bounds.Height;
            width = height * imageAspect;
        }
        else
        {
            width = bounds.Width;
            height = width / imageAspect;
        }

        var x = bounds.X + (bounds.Width - width) / 2f;
        var y = bounds.Y + (bounds.Height - height) / 2f;
        canvas.DrawImage(image, x, y, width, height);
    }

    private static IImage? LoadTitleImage()
    {
        var assembly = typeof(TitleScene).Assembly;
        using var stream = assembly.GetManifestResourceStream("ChooseYourOwnAdventure.TitleScreen.png");
        return stream is null ? null : PlatformImage.FromStream(stream);
    }

    private static void DrawFallback(ICanvas canvas, RectF dimensions)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Color.FromArgb("#D83A16");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.05f, 38f, 76f);
        canvas.DrawString(
            "CHOOSE YOUR OWN ADVENTURE",
            dimensions.X + 50f,
            dimensions.Y + dimensions.Height * 0.30f,
            dimensions.Width - 100f,
            90f,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Color.FromArgb("#44251C");
        canvas.FontSize = Math.Clamp(dimensions.Width * 0.019f, 16f, 28f);
        canvas.DrawString(
            "Building interactive games with .NET MAUI",
            dimensions.X + 60f,
            dimensions.Y + dimensions.Height * 0.46f,
            dimensions.Width - 120f,
            44f,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);
    }
}
