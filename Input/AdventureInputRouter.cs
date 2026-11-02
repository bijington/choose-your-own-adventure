namespace ChooseYourOwnAdventure.Input;

/// <summary>
/// Converts physical input sources into semantic actions understood by the talk.
/// It exposes both edge-triggered actions (useful for slide/navigation commands)
/// and held state (useful for real game input such as acceleration).
/// </summary>
public sealed class AdventureInputRouter
{
    private readonly HashSet<AdventureAction> _pressed = [];

    public event EventHandler<AdventureAction>? ActionTriggered;
    public event EventHandler<AdventureActionStateChangedEventArgs>? ActionStateChanged;

    public bool IsPressed(AdventureAction action) => _pressed.Contains(action);

    public void Trigger(AdventureAction action) =>
        ActionTriggered?.Invoke(this, action);

    public void SetPressed(AdventureAction action, bool isPressed)
    {
        var changed = isPressed
            ? _pressed.Add(action)
            : _pressed.Remove(action);

        if (!changed)
        {
            return;
        }

        ActionStateChanged?.Invoke(this, new AdventureActionStateChangedEventArgs(action, isPressed));

        if (isPressed)
        {
            ActionTriggered?.Invoke(this, action);
        }
    }

    public async Task PulseAsync(AdventureAction action, TimeSpan duration)
    {
        SetPressed(action, true);
        try
        {
            await Task.Delay(duration);
        }
        finally
        {
            SetPressed(action, false);
        }
    }
}

public sealed class AdventureActionStateChangedEventArgs : EventArgs
{
    public AdventureActionStateChangedEventArgs(AdventureAction action, bool isPressed)
    {
        Action = action;
        IsPressed = isPressed;
    }

    public AdventureAction Action { get; }
    public bool IsPressed { get; }
}
