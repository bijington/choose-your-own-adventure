using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.GameObjects;

// Scenes supply placement; all vehicle artwork lives in this GameObject.
public abstract class ChampionVehicleGameObject : GameObject
{
    protected static void DrawVehicle(ICanvas canvas, Champion champion, float steering = 0f, bool thrusting = false)
    {
        switch (champion)
        {
            case Champion.RacingDriver: DrawCar(canvas, steering); break;
            case Champion.Charioteer: DrawChariot(canvas); break;
            case Champion.SpacePilot: DrawShip(canvas, thrusting); break;
        }
    }

    protected static void DrawCar(ICanvas canvas, float steering)
    {
        // Wheels visibly steer while the body continues along the track tangent.
        DrawWheel(canvas, -14, -13, steering * 18f);
        DrawWheel(canvas, -14, 7, steering * 18f);
        DrawWheel(canvas, 10, -13, 0f);
        DrawWheel(canvas, 10, 7, 0f);

        canvas.FillColor = Color.FromArgb("#EF4444");
        canvas.FillRoundedRectangle(new RectF(-22, -9, 42, 18), 6);
        canvas.FillRectangle(-26, -13, 7, 26);
        canvas.FillRectangle(15, -15, 5, 30);
        canvas.FillColor = Color.FromArgb("#F8FAFC");
        canvas.FillRoundedRectangle(new RectF(-5, -6, 13, 12), 5);
    }

    protected static void DrawWheel(ICanvas canvas, float x, float y, float rotation)
    {
        canvas.SaveState();
        canvas.Translate(x + 5, y + 5);
        canvas.Rotate(rotation);
        canvas.FillColor = Color.FromArgb("#111827");
        canvas.FillRoundedRectangle(new RectF(-5, -6, 10, 12), 3);
        canvas.RestoreState();
    }

    protected static void DrawChariot(ICanvas canvas)
    {
        canvas.StrokeColor = Color.FromArgb("#F8D866");
        canvas.StrokeSize = 3;
        canvas.DrawLine(-3, -8, 17, -10);
        canvas.DrawLine(-3, 8, 17, 10);
        canvas.FillColor = Color.FromArgb("#B45309");
        canvas.FillRoundedRectangle(new RectF(-23, -12, 22, 24), 5);
        canvas.FillColor = Color.FromArgb("#1E293B");
        canvas.FillEllipse(-19, -18, 10, 10);
        canvas.FillEllipse(-19, 8, 10, 10);
        canvas.FillColor = Color.FromArgb("#A16207");
        canvas.FillRoundedRectangle(new RectF(14, -15, 21, 10), 5);
        canvas.FillRoundedRectangle(new RectF(14, 5, 21, 10), 5);
        canvas.FillEllipse(29, -17, 8, 8);
        canvas.FillEllipse(29, 9, 8, 8);
    }

    protected static void DrawShip(ICanvas canvas, bool thrusting)
    {
        if (thrusting)
        {
            canvas.FillColor = Color.FromArgb("#FBBF24");
            canvas.FillRectangle(-37, -6, 18, 4);
            canvas.FillRectangle(-37, 2, 18, 4);
            canvas.FillColor = Color.FromArgb("#F97316");
            canvas.FillRectangle(-31, -8, 12, 5);
            canvas.FillRectangle(-31, 3, 12, 5);
        }
        else
        {
            canvas.FillColor = Color.FromArgb("#F97316");
            canvas.FillRectangle(-27, -6, 8, 4);
            canvas.FillRectangle(-27, 2, 8, 4);
        }

        canvas.FillColor = Color.FromArgb("#6366F1");
        var body = new PathF();
        body.MoveTo(30, 0);
        body.LineTo(-16, -14);
        body.LineTo(-7, 0);
        body.LineTo(-16, 14);
        body.Close();
        canvas.FillPath(body);

        canvas.FillColor = Color.FromArgb("#818CF8");
        var wings = new PathF();
        wings.MoveTo(3, 0);
        wings.LineTo(-17, -24);
        wings.LineTo(-12, -4);
        wings.LineTo(-17, 24);
        wings.Close();
        canvas.FillPath(wings);

        canvas.FillColor = Color.FromArgb("#22D3EE");
        canvas.FillEllipse(2, -6, 12, 12);
    }
}
