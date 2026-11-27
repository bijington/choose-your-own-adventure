using ChooseYourOwnAdventure.Presentation;
using Microsoft.Maui.Graphics;

namespace ChooseYourOwnAdventure.Scenes;

public sealed class InputOverviewScene(AdventureState state) : PresentationSceneBase
{
    public override void Render(ICanvas canvas, RectF dimensions)
    {
        DrawShell(canvas, dimensions);
        DrawTitle(canvas, dimensions, "MANY WAYS TO PLAY",
            "A tap, a tilt, a turn… or a controller. Different sources, the same intent.");
        var left = BookLayout.LeftPage(dimensions);
        var right = BookLayout.RightPage(dimensions);
        DrawColumn(canvas, left, "MAUI UI + DEVICE SENSORS",
            "TOUCH + MOUSE", "Buttons, commands, taps, swipes and pointer input.",
            "MOVE THE DEVICE", "Accelerometer: movement and shake.\nGyroscope: rotation speed.",
            "FIND YOUR BEARINGS", "Orientation sensor: device attitude.\nCompass: heading.");
        DrawColumn(canvas, right, "KEYBOARD + GAME CONTROLLERS",
            "KEYS", "Actions, directions and held controls.",
            "BUTTONS + STICKS", "Throttle, brake, steer… or something stranger.",
            "ORBIT.INPUT", "A shared input layer for our game.\nAlongside ordinary MAUI UI.");
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontColor = Muted;
        canvas.FontSize = Math.Clamp(dimensions.Width * .012f, 12, 18);
        canvas.DrawString("Sensors depend on hardware and platform support. No sensors are activated by this slide.",
            left.X, dimensions.Y + dimensions.Height * .85f, dimensions.Width * .87f, 30,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        DrawPrompt(canvas, dimensions, "SOUTH / RB / ENTER / PGDN · START WITH A MAUI BUTTON");
        DrawPageNumber(canvas, dimensions, PresentationTheme.GetPageNumber(PresentationSlide.InputOverview, state.Champion));
    }

    private static void DrawColumn(ICanvas canvas, RectF page, string heading,
        string first, string firstBody, string second, string secondBody, string third, string thirdBody)
    {
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontColor = Accent;
        canvas.FontSize = Math.Clamp(page.Width * .04f, 16, 24);
        canvas.DrawString(heading, page.X, page.Y, page.Width, page.Height * .12f,
            HorizontalAlignment.Left, VerticalAlignment.Center);
        var entries = new[] { (first, firstBody), (second, secondBody), (third, thirdBody) };
        for (var i = 0; i < entries.Length; i++)
        {
            var y = page.Y + page.Height * (.16f + i * .27f);
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontColor = Ink;
            canvas.FontSize = Math.Clamp(page.Width * .038f, 15, 22);
            canvas.DrawString(entries[i].Item1, page.X, y, page.Width, page.Height * .07f,
                HorizontalAlignment.Left, VerticalAlignment.Center);
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;
            canvas.FontColor = Muted;
            canvas.FontSize = Math.Clamp(page.Width * .034f, 14, 20);
            canvas.DrawString(entries[i].Item2, page.X, y + page.Height * .09f, page.Width, page.Height * .15f,
                HorizontalAlignment.Left, VerticalAlignment.Top);
        }
    }
}
