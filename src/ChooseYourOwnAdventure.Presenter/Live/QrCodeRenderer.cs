using Microsoft.Maui.Graphics;
using QRCoder;

namespace ChooseYourOwnAdventure.Live;

public sealed class QrCodeRenderer : IDisposable
{
    private readonly QRCodeData _data;

    public QrCodeRenderer(LiveAudienceOptions options)
    {
        using var generator = new QRCodeGenerator();
        _data = generator.CreateQrCode(options.JoinUrl, QRCodeGenerator.ECCLevel.Q);
    }

    public void Draw(ICanvas canvas, RectF bounds)
    {
        var modules = _data.ModuleMatrix;
        if (modules.Count == 0)
        {
            return;
        }

        canvas.FillColor = Colors.White;
        canvas.FillRectangle(bounds);

        var moduleSize = Math.Min(bounds.Width, bounds.Height) / modules.Count;
        var size = moduleSize * modules.Count;
        var originX = bounds.X + (bounds.Width - size) / 2f;
        var originY = bounds.Y + (bounds.Height - size) / 2f;

        canvas.FillColor = Colors.Black;
        for (var y = 0; y < modules.Count; y++)
        {
            for (var x = 0; x < modules[y].Count; x++)
            {
                if (modules[y][x])
                {
                    canvas.FillRectangle(originX + x * moduleSize, originY + y * moduleSize,
                        moduleSize + 0.5f, moduleSize + 0.5f);
                }
            }
        }
    }

    public void Dispose() => _data.Dispose();
}
