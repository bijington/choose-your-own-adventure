namespace ChooseYourOwnAdventure.Input;

/// <summary>
/// Converts physical input sources into semantic actions understood by the talk.
/// Touch, a game controller and later SignalR can all feed this same router.
/// </summary>
public sealed class AdventureInputRouter
{
    public event EventHandler<AdventureAction>? ActionTriggered;

    public void Trigger(AdventureAction action) =>
        ActionTriggered?.Invoke(this, action);
}
