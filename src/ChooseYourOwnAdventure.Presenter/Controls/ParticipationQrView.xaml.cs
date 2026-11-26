using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Controls;

public partial class ParticipationQrView : ContentView
{
    private CodeDrawable? _drawable;
    public event EventHandler? DismissRequested;
    public ParticipationQrView() => InitializeComponent();
    public void Configure(QrCodeRenderer qr, LiveAudienceOptions options) =>
        CodeCanvas.Drawable = _drawable = new CodeDrawable(qr, options);
    public void Refresh(string destination)
    {
        if (_drawable is not null) _drawable.Destination = destination;
        CodeCanvas.Invalidate();
    }
    private void OnDismissTapped(object? sender, TappedEventArgs e) => DismissRequested?.Invoke(this, EventArgs.Empty);

    private sealed class CodeDrawable(QrCodeRenderer qr, LiveAudienceOptions options) : IDrawable
    {
        public string Destination { get; set; } = "the presentation";
        public void Draw(ICanvas canvas, RectF bounds)
        {
            PresentationTheme.DrawParchment(canvas, bounds);
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontColor = PresentationTheme.Ink;
            canvas.FontSize = Math.Clamp(bounds.Width * .032f, 24, 44);
            canvas.DrawString("JOIN THE ADVENTURE", bounds.X + bounds.Width * .05f, bounds.Y + bounds.Height * .045f,
                bounds.Width * .90f, bounds.Height * .075f, HorizontalAlignment.Center, VerticalAlignment.Center);
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;
            canvas.FontColor = PresentationTheme.MutedInk;
            canvas.FontSize = Math.Clamp(bounds.Width * .016f, 14, 24);
            canvas.DrawString("Scan with your phone. Keep the same browser open to stay on your team.",
                bounds.X + bounds.Width * .05f, bounds.Y + bounds.Height * .12f, bounds.Width * .90f, bounds.Height * .045f,
                HorizontalAlignment.Center, VerticalAlignment.Center);
            var size = Math.Min(bounds.Width * .76f, bounds.Height * .62f);
            qr.Draw(canvas, new RectF(bounds.Center.X - size / 2, bounds.Y + bounds.Height * .18f, size, size));
            canvas.FontColor = PresentationTheme.Ink;
            canvas.FontSize = Math.Clamp(bounds.Width * .016f, 14, 24);
            canvas.DrawString(options.JoinUrl, bounds.X + bounds.Width * .05f, bounds.Y + bounds.Height * .82f,
                bounds.Width * .90f, bounds.Height * .06f, HorizontalAlignment.Center, VerticalAlignment.Center);
            canvas.FontColor = PresentationTheme.AccentRed;
            canvas.FontSize = Math.Clamp(bounds.Width * .014f, 12, 20);
            canvas.DrawString($"Tap / South / Enter / shoulder · return to {Destination}",
                bounds.X + bounds.Width * .05f, bounds.Y + bounds.Height * .91f, bounds.Width * .90f, bounds.Height * .045f,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}
