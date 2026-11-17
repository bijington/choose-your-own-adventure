using Microsoft.Maui.Graphics;
using Orbit.Engine;

namespace ChooseYourOwnAdventure.GameObjects;

// Scenes supply placement; all moving vehicles share the top-down sprite renderer.
public abstract class ChampionVehicleGameObject : GameObject
{
    protected static void DrawVehicle(ICanvas canvas, Champion champion, float steering = 0f, bool thrusting = false) =>
        ChampionSprites.DrawTopVehicle(canvas, champion, steering, thrusting);

    protected static void DrawCar(ICanvas canvas, float steering) =>
        DrawVehicle(canvas, Champion.RacingDriver, steering);

    protected static void DrawChariot(ICanvas canvas) =>
        DrawVehicle(canvas, Champion.Charioteer);

    protected static void DrawShip(ICanvas canvas, bool thrusting) =>
        DrawVehicle(canvas, Champion.SpacePilot, thrusting: thrusting);
}
