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
