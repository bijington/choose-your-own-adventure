using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.GameObjects;

public sealed class LoopRacer : GameObject
{
    private readonly AdventureState _state;
    private double _progress;

    public LoopRacer(AdventureState state)
    {
        _state = state;
    }

    public double Progress => _progress;

    public override void Update(double millisecondsSinceLastUpdate)
    {
        base.Update(millisecondsSinceLastUpdate);

        // This is the important bit: movement happens in Orbit's update loop.
        _progress = (_progress + millisecondsSinceLastUpdate * 0.00010d) % 1d;
    }

    public override void Render(ICanvas canvas, RectF dimensions)
    {
        base.Render(canvas, dimensions);

        var track = GetTrackBounds(dimensions);
        var angle = _progress * Math.PI * 2d - Math.PI / 2d;
        var rx = track.Width * 0.43f;
        var ry = track.Height * 0.38f;
        var cx = (track.X + track.Width / 2f) + (float)Math.Cos(angle) * rx;
        var cy = (track.Y + track.Height / 2f) + (float)Math.Sin(angle) * ry;

        switch (_state.Champion)
        {
            case Champion.Charioteer:
                DrawChariot(canvas, cx, cy);
                break;
            case Champion.SpacePilot:
                DrawShip(canvas, cx, cy);
                break;
            default:
                DrawCar(canvas, cx, cy);
                break;
        }
    }

    private static RectF GetTrackBounds(RectF dimensions)
    {
        var width = dimensions.Width * 0.62f;
        var height = dimensions.Height * 0.48f;

        return new RectF(
            dimensions.X + (dimensions.Width - width) / 2f,
            dimensions.Y + dimensions.Height * 0.20f,
            width,
            height);
    }

    private static void DrawCar(ICanvas canvas, float x, float y)
    {
        canvas.FillColor = Color.FromArgb("#EF4444");
        canvas.FillRoundedRectangle(new RectF(x - 22, y - 11, 44, 22), 6);
        canvas.FillColor = Color.FromArgb("#F8FAFC");
        canvas.FillRectangle(x - 8, y - 15, 16, 9);
        canvas.FillColor = Color.FromArgb("#0F172A");
        canvas.FillEllipse(x - 16, y + 6, 9, 9);
        canvas.FillEllipse(x + 7, y + 6, 9, 9);
    }

    private static void DrawChariot(ICanvas canvas, float x, float y)
    {
        canvas.FillColor = Color.FromArgb("#D97706");
        canvas.FillRoundedRectangle(new RectF(x - 25, y - 8, 28, 18), 4);
        canvas.FillColor = Color.FromArgb("#A16207");
        canvas.FillRoundedRectangle(new RectF(x + 1, y - 6, 26, 11), 5);
        canvas.FillColor = Color.FromArgb("#1E293B");
        canvas.FillEllipse(x - 19, y + 5, 10, 10);
        canvas.FillEllipse(x - 3, y + 5, 10, 10);
        canvas.FillColor = Color.FromArgb("#F8D866");
        canvas.FillEllipse(x - 17, y - 20, 9, 9);
    }

    private static void DrawShip(ICanvas canvas, float x, float y)
    {
        canvas.FillColor = Color.FromArgb("#6366F1");
        var path = new PathF();
        path.MoveTo(x + 25, y);
        path.LineTo(x - 18, y - 14);
        path.LineTo(x - 10, y);
        path.LineTo(x - 18, y + 14);
        path.Close();
        canvas.FillPath(path);
        canvas.FillColor = Color.FromArgb("#22D3EE");
        canvas.FillEllipse(x - 1, y - 5, 10, 10);
        canvas.FillColor = Color.FromArgb("#F97316");
        canvas.FillRectangle(x - 23, y - 4, 9, 8);
    }
}
