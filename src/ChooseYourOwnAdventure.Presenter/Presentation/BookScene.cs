using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.Presentation;

/// <summary>One Orbit loop, with a frozen paper leaf during scene changes.</summary>
public sealed class BookScene : GameScene, IDisposable
{
    private const double TurnDuration = 680;
    private GameScene? _content;
    private PlatformBitmapExportContext? _paper;
    private Microsoft.Maui.Graphics.IImage? _paperImage;
    private PlatformBitmapExportContext? _incomingPaper;
    private Microsoft.Maui.Graphics.IImage? _incomingImage;
    private RectF _dimensions;
    private double _turnElapsed;
    private bool _backwards;
    private bool _startTurnOnNextUpdate;

    public bool IsTurning => _paperImage is not null;

    public void CapturePage(bool backwards)
    {
        ReleasePaper();
        if (_content is null || _dimensions.Width <= 0 || _dimensions.Height <= 0) return;
        try
        {
            _paper = new PlatformBitmapExportContext((int)Math.Ceiling(_dimensions.Width),
                (int)Math.Ceiling(_dimensions.Height), 1);
            _content.Render(_paper.Canvas, new RectF(0, 0, _dimensions.Width, _dimensions.Height));
            _paperImage = _paper.Image;
            _backwards = backwards;
            _turnElapsed = 0;
        }
        catch (Exception exception)
        {
            // A missing capture implementation must never prevent navigation.
            System.Diagnostics.Debug.WriteLine($"Page capture unavailable: {exception.Message}");
            ReleasePaper();
        }
    }

    public void Show(GameScene content)
    {
        _content = content;
        if (!IsTurning) return;
        try
        {
            _incomingPaper = new PlatformBitmapExportContext((int)Math.Ceiling(_dimensions.Width),
                (int)Math.Ceiling(_dimensions.Height), 1);
            content.Render(_incomingPaper.Canvas, new RectF(0, 0, _dimensions.Width, _dimensions.Height));
            _incomingImage = _incomingPaper.Image;
            _startTurnOnNextUpdate = true;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"Incoming page capture unavailable: {exception.Message}");
            ReleasePaper();
        }
    }

    public override void Update(double millisecondsSinceLastUpdate)
    {
        // Capturing two spreads can take longer than a normal frame. Start the
        // animation clock afterwards, so that work cannot skip the whole turn.
        if (_startTurnOnNextUpdate)
        {
            _startTurnOnNextUpdate = false;
            return;
        }
        _content?.Update(millisecondsSinceLastUpdate);
        if (!IsTurning) return;
        _turnElapsed += millisecondsSinceLastUpdate;
        if (_turnElapsed >= TurnDuration) ReleasePaper();
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        _dimensions = dimensions;
        canvas.SaveState();
        _content?.Render(canvas, dimensions);
        canvas.RestoreState();
        if (_paperImage is null) return;

        var progress = (float)Math.Clamp(_turnElapsed / TurnDuration, 0, 1);
        var cosine = (float)Math.Cos(progress * Math.PI);
        var centre = dimensions.X + dimensions.Width / 2;
        var half = dimensions.Width / 2;
        var direction = _backwards ? -1f : 1f;

        // Keep the old stationary page underneath until the new leaf lands.
        // Revealing the new page early would show its content twice during the fold.
        {
            canvas.SaveState();
            canvas.ClipRectangle(_backwards ? centre : dimensions.X, dimensions.Y, half, dimensions.Height);
            canvas.DrawImage(_paperImage, dimensions.X, dimensions.Y, dimensions.Width, dimensions.Height);
            canvas.RestoreState();
        }

        var pageWidth = Math.Max(0, half - 30);
        var leafWidth = pageWidth * Math.Abs(cosine);
        var leafX = cosine * direction >= 0 ? centre : centre - leafWidth;
        var leaf = new RectF(leafX, dimensions.Y + 18, leafWidth, dimensions.Height - 48);
        // Moving cast shadow makes the flat illustration read as a lifted sheet of paper.
        canvas.SaveState();
        canvas.Alpha = .18f * (float)Math.Sin(progress * Math.PI);
        canvas.FillColor = Colors.Black;
        canvas.FillRectangle(leafX - 12, leaf.Y + 6, leafWidth + 24, leaf.Height);
        canvas.RestoreState();

        // Front is the outgoing right page; reverse is the incoming left page.
        // Reverse navigation swaps those leaves. Positive scaling keeps the text readable.
        var front = progress < .5f;
        var image = front ? _paperImage : _incomingImage;
        if (image is not null)
        {
            canvas.SaveState();
            canvas.Translate(centre, dimensions.Y);
            canvas.Scale(Math.Max(.002f, Math.Abs(cosine)), 1);
            var onLeft = front ? _backwards : !_backwards;
            canvas.ClipRectangle(onLeft ? -pageWidth : 0, 18, pageWidth, dimensions.Height - 48);
            canvas.DrawImage(image, -half, 0, dimensions.Width, dimensions.Height);
            canvas.RestoreState();
        }

        canvas.SaveState();
        for (var i = 0; i < 16; i++)
        {
            canvas.Alpha = (1 - i / 16f) * .14f * (float)Math.Sin(progress * Math.PI);
            canvas.FillColor = PresentationTheme.Ink;
            canvas.FillRectangle(leafX + (direction * cosine >= 0 ? i : leafWidth - i), leaf.Y, 1, leaf.Height);
        }
        canvas.RestoreState();
    }

    private void ReleasePaper()
    {
        _startTurnOnNextUpdate = false;
        _incomingImage?.Dispose();
        _incomingImage = null;
        _incomingPaper?.Dispose();
        _incomingPaper = null;
        _paperImage?.Dispose();
        _paperImage = null;
        _paper?.Dispose();
        _paper = null;
    }

    public void Dispose() => ReleasePaper();
}
