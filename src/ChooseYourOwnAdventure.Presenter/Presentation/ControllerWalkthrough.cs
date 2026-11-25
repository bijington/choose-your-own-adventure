namespace ChooseYourOwnAdventure.Presentation;

public enum ControllerStage { Setup, Discovery, Connected, Actions }
public sealed record ControllerStep(string Title, string CodeTitle, string Code, string Explanation, string TalkingPoints);

// Focused excerpts of MauiProgram and OrbitInputBridge; runtime input is unchanged.
public static class ControllerWalkthrough
{
    public static ControllerStep ForStage(ControllerStage stage) => stage switch
    {
        ControllerStage.Setup => new("Register input support", "NuGet + MauiProgram.cs", """
// dotnet add package Bijington.Orbit.Input
builder
    .UseMauiApp<App>()
    .UseOrbitEngine()
    .UseOrbitGameController()
    .UseOrbitKeyboard();
""", "Add the package. Register controller and keyboard support.\n\nOur bridge receives the managers through dependency injection.",
            "Start with Bijington.Orbit.Input and the existing builder registrations in MauiProgram. The bridge receives GameControllerManager and KeyboardManager through DI. This sets up support; discovery is the next step."),
        ControllerStage.Discovery => new("Listen, then discover", "OrbitInputBridge · StartControllerInputAsync", """
_gameControllerManager
    .GameControllerConnected +=
        GameControllerManagerOnGameControllerConnected;

await _gameControllerManager.StartDiscovery();
""", "Subscribe before starting discovery.\n\nWhen a controller connects, the manager raises GameControllerConnected.",
            "This is the order used by our bridge: subscribe to GameControllerConnected before awaiting StartDiscovery. We register input before discovering hardware. The real method handles exceptions so controller discovery never prevents keyboard or touch from working."),
        ControllerStage.Connected => new("Keep the controller; hook its events", "OrbitInputBridge · connected handler", """
// From GameControllerConnectedEventArgs e:
_gameController = e.GameController;

e.GameController.ButtonChanged +=
    GameControllerOnButtonChanged;
e.GameController.ValueChanged +=
    GameControllerOnValueChanged;
""", "The event gives us the controller instance.\n\nKeep it for state access. Hook button and analog-value changes.",
            "The connected event’s e.GameController is how we get access to the controller. Store that instance, then hook ButtonChanged and ValueChanged. The full handler detaches the previous controller and clears held actions before replacing it; Dispose removes the subscriptions too."),
        ControllerStage.Actions => new("Read the current input values", "Connected controller · simple state access", """
// Button: pressed or released
bool jump = gameController.South.Value;

// Shoulder trigger: 0 to 1
float throttle =
    gameController.RightShoulder.Trigger.Value;

// Stick axis: -1 to 1
float steering =
    gameController.LeftStick.XAxis.Value;
""", "BUTTON · true / false\nTRIGGER · 0 released → 1 fully pressed\nAXIS · -1 left → 0 centre → 1 right\n\nRead the values. Decide what they mean for your game.",
            "We already have a connected controller from the previous step. Now just read its current values: South.Value is a boolean, RightShoulder.Trigger.Value is an analog value from 0 to 1, and LeftStick.XAxis.Value runs from -1 to 1. These could mean jump, throttle and steering. We can react to events or read state through the controller instance; the game decides what those values mean. This is a small teaching example, not the presentation’s full routing code."),
        _ => throw new ArgumentOutOfRangeException(nameof(stage))
    };
}
