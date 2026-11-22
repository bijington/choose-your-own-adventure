using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace ChooseYourOwnAdventure.GameObjects;

// Decode once per asset, rather than opening PNGs on every animation frame.
internal static class ChampionSprites
{
    private static readonly Dictionary<string, Lazy<IImage>> Images = new();

    private static IImage Get(Champion champion, string view)
    {
        var name = champion switch
        {
            Champion.Charioteer => "charioteer",
            Champion.SpacePilot => "pilot",
            _ => "driver"
        } + "_" + view;

        Lazy<IImage> image;
        lock (Images)
        {
            if (!Images.TryGetValue(name, out image!))
            {
                image = new Lazy<IImage>(() =>
                {
                    using var stream = typeof(ChampionSprites).Assembly.GetManifestResourceStream(
                        $"ChooseYourOwnAdventure.Champions.{name}.png")
                        ?? throw new InvalidOperationException($"Missing champion sprite: {name}");
                    return PlatformImage.FromStream(stream);
                });
                Images.Add(name, image);
            }
        }
        return image.Value;
    }

    public static void DrawCharacter(ICanvas canvas, Champion champion, RectF bounds) =>
        DrawFit(canvas, Get(champion, "character"), bounds);

    // Each scene gets its own playback state; decoded frame images remain shared.
    public static Orbit.Engine.Sprite CreateCelebration(Champion champion) => new(
        Enumerable.Range(0, 8).Select(frame => Get(champion, $"celebrate_{frame:00}")).ToArray(),
        imageDisplayDuration: 125d,
        autoStart: true,
        playMode: Orbit.Engine.Sprite.PlayModeType.Loop);

    private static readonly Dictionary<Champion, IImage[]> MotionFrames = new();

    public static Orbit.Engine.Sprite CreateMotion(Champion champion)
    {
        if (!MotionFrames.TryGetValue(champion, out var frames))
        {
            frames = Enumerable.Range(0, 4).Select(frame => CreateMotionFrame(champion, frame)).ToArray();
            MotionFrames.Add(champion, frames);
        }
        return new Orbit.Engine.Sprite(frames, imageDisplayDuration: 90d, autoStart: true,
            playMode: Orbit.Engine.Sprite.PlayModeType.Loop);
    }

    private static IImage CreateMotionFrame(Champion champion, int frame)
    {
        // Build transparent animation frames once from the existing artwork and pixel details.
        using var context = new PlatformBitmapExportService().CreateContext(128, 160, 1);
        var canvas = context.Canvas;
        canvas.Translate(64, 80);
        canvas.Antialias = false;
        var length = champion == Champion.Charioteer ? 76f : 64f;
        if (champion == Champion.SpacePilot)
        {
            var flame = new PathF();
            flame.MoveTo(-7, 25);
            flame.LineTo(frame % 2 == 0 ? -3 : 2, 47 + frame * 3);
            flame.LineTo(7, 25);
            flame.Close();
            canvas.FillColor = Color.FromArgb("#E96225");
            canvas.FillPath(flame);
            canvas.FillColor = Color.FromArgb("#FFCE56");
            canvas.FillRectangle(-3, 27, 6, 13 + frame * 2);
        }
        if (champion == Champion.Charioteer)
        {
            canvas.FillColor = Color.FromArgb("#BC9862");
            for (var puff = 0; puff < 3; puff++)
            {
                var y = 40 + (frame * 4 + puff * 6) % 20;
                canvas.FillRectangle(-12 + puff * 9, y, 3, 3);
            }
        }
        DrawFit(canvas, Get(champion, "top"), new RectF(-22, -length / 2, 44, length));
        if (champion == Champion.RacingDriver)
        {
            canvas.FillColor = Color.FromArgb("#ADA4A3");
            foreach (var x in new[] { -18f, 11f })
            foreach (var y in new[] { -17f, 17f })
            for (var tread = 0; tread < 3; tread++)
                canvas.FillRectangle(x, y + (tread * 3 + frame * 2) % 9, 7, 1);
        }
        if (champion == Champion.Charioteer)
        {
            canvas.FillColor = Color.FromArgb("#E2AD58");
            foreach (var x in new[] { -17f, 14f })
            for (var spoke = 0; spoke < 3; spoke++)
                canvas.FillRectangle(x, 22 + (spoke * 4 + frame * 2) % 12, 3, 1);
        }
        using var stream = new MemoryStream();
        context.WriteToStream(stream);
        stream.Position = 0;
        return PlatformImage.FromStream(stream);
    }

    public static void DrawSideVehicle(ICanvas canvas, Champion champion, RectF bounds) =>
        DrawFit(canvas, Get(champion, "side"), bounds);

    // The source images face up; existing course headings use right as zero degrees.
    public static void DrawTopVehicle(ICanvas canvas, Champion champion, float steering = 0f, bool thrusting = false)
    {
        canvas.SaveState();
        try
        {
            canvas.Rotate(90f + (champion == Champion.RacingDriver ? steering * 8f : 0f));
            var length = champion == Champion.Charioteer ? 76f : 64f;
            if (champion == Champion.SpacePilot && thrusting) length *= 1.08f;
            DrawFit(canvas, Get(champion, "top"), new RectF(-22f, -length / 2f, 44f, length));
        }
        finally { canvas.RestoreState(); }
    }

    private static void DrawFit(ICanvas canvas, IImage image, RectF bounds)
    {
        var scale = Math.Min(bounds.Width / image.Width, bounds.Height / image.Height);
        var width = image.Width * scale;
        var height = image.Height * scale;
        canvas.SaveState();
        try
        {
            canvas.Antialias = false;
            canvas.DrawImage(image, bounds.X + (bounds.Width - width) / 2f,
                bounds.Y + (bounds.Height - height) / 2f, width, height);
        }
        finally { canvas.RestoreState(); }
    }
}
