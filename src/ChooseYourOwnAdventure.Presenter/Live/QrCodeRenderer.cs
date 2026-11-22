using Microsoft.Maui.Graphics;
using QRCoder;

namespace ChooseYourOwnAdventure.Live;

public sealed class QrCodeRenderer : IDisposable
{
    private QRCodeData _data;
    private readonly LiveAudienceOptions? _options;
    private string _url;

    public QrCodeRenderer(LiveAudienceOptions options) : this(options.JoinUrl) => _options = options;

    public static QrCodeRenderer ForUrl(string url) => new(url);

    private QrCodeRenderer(string url)
    {
        _url = url;
        using var generator = new QRCodeGenerator();
        _data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
    }

    public void Draw(ICanvas canvas, RectF bounds)
    {
        if (_options is not null && _url != _options.JoinUrl)
        {
            using var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(_options.JoinUrl, QRCodeGenerator.ECCLevel.Q);
            _data.Dispose();
            _data = data;
            _url = _options.JoinUrl;
        }
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
