using Microsoft.Maui.ApplicationModel;
using Orbit.Input;

namespace ChooseYourOwnAdventure.Input;

/// <summary>
/// Bridges Orbit.Input keyboard and game-controller state into the semantic
/// AdventureAction values used by the presentation.
/// </summary>
public sealed class OrbitInputBridge : IDisposable
{
    private const float StickDeadZone = 0.55f;

    private readonly AdventureInputRouter _router;
    private readonly GameControllerManager _gameControllerManager;
    private readonly KeyboardManager _keyboardManager;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private Task? _pollingTask;
    private bool _started;
    private bool _southWasPressed;
    private bool _eastWasPressed;
    private int _horizontalDirection;
    private int _verticalDirection;

    public OrbitInputBridge(
        AdventureInputRouter router,
        GameControllerManager gameControllerManager,
        KeyboardManager keyboardManager)
    {
        _router = router;
        _gameControllerManager = gameControllerManager;
        _keyboardManager = keyboardManager;
    }

    public Task StartAsync()
    {
        if (_started)
        {
            return Task.CompletedTask;
        }

        _started = true;
        _keyboardManager.KeyDown += OnKeyDown;
        _keyboardManager.KeyUp += OnKeyUp;

        _pollingTask = StartControllerInputAsync(_cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    private async Task StartControllerInputAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _gameControllerManager.StartDiscovery();

            if (!cancellationToken.IsCancellationRequested)
            {
                await PollControllerAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            // Controller support is optional. Keyboard/touch must remain usable
            // even if platform discovery is unavailable or fails.
        }
    }

    private void OnKeyDown(object? sender, KeyboardKeyChangeEventArgs key)
    {
        var action = MapKey(key.Key);
        if (action is not null)
        {
            SetPressed(action.Value, true);
        }
    }

    private void OnKeyUp(object? sender, KeyboardKeyChangeEventArgs key)
    {
        var action = MapKey(key.Key);
        if (action is not null)
        {
            SetPressed(action.Value, false);
        }
    }

    private static AdventureAction? MapKey(KeyboardKey key)
    {
        var name = key.ToString();

        if (Matches(name, "KeyA", "ArrowLeft", "LeftArrow", "Left"))
        {
            return AdventureAction.Left;
        }

        if (Matches(name, "KeyD", "ArrowRight", "RightArrow", "Right"))
        {
            return AdventureAction.Right;
        }

        if (Matches(name, "KeyW", "ArrowUp", "UpArrow", "Up"))
        {
            return AdventureAction.Up;
        }

        if (Matches(name, "KeyS", "ArrowDown", "DownArrow", "Down"))
        {
            return AdventureAction.Down;
        }

        if (Matches(name, "Enter", "Return", "Space", "Spacebar"))
        {
            return AdventureAction.Primary;
        }

        if (Matches(name, "ShiftLeft", "ShiftRight"))
        {
            return AdventureAction.Secondary;
        }

        if (Matches(name, "Escape", "Backspace"))
        {
            return AdventureAction.Back;
        }

        return null;
    }

    private async Task PollControllerAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(16));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var controller = _gameControllerManager.GameControllers.FirstOrDefault();
                if (controller is null)
                {
                    ResetControllerEdges();
                    continue;
                }

                PollButtons(controller);
                PollLeftStick(controller);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void PollButtons(Orbit.Input.GameController controller)
    {
        var southPressed = controller.South.Value;
        if (southPressed != _southWasPressed)
        {
            SetPressed(AdventureAction.Primary, southPressed);
            _southWasPressed = southPressed;
        }

        var eastPressed = controller.East.Value;
        if (eastPressed != _eastWasPressed)
        {
            SetPressed(AdventureAction.Secondary, eastPressed);
            _eastWasPressed = eastPressed;
        }
    }

    private void PollLeftStick(Orbit.Input.GameController controller)
    {
        var x = controller.LeftStick.XAxis.Value;
        var horizontalDirection = x switch
        {
            < -StickDeadZone => -1,
            > StickDeadZone => 1,
            _ => 0
        };

        if (horizontalDirection != _horizontalDirection)
        {
            SetPressed(AdventureAction.Left, horizontalDirection < 0);
            SetPressed(AdventureAction.Right, horizontalDirection > 0);
            _horizontalDirection = horizontalDirection;
        }

        var y = controller.LeftStick.YAxis.Value;
        var verticalDirection = y switch
        {
            < -StickDeadZone => -1,
            > StickDeadZone => 1,
            _ => 0
        };

        if (verticalDirection != _verticalDirection)
        {
            SetPressed(AdventureAction.Up, verticalDirection < 0);
            SetPressed(AdventureAction.Down, verticalDirection > 0);
            _verticalDirection = verticalDirection;
        }
    }

    private void SetPressed(AdventureAction action, bool isPressed)
    {
        MainThread.BeginInvokeOnMainThread(() => _router.SetPressed(action, isPressed));
    }

    private void ResetControllerEdges()
    {
        if (_southWasPressed)
        {
            SetPressed(AdventureAction.Primary, false);
        }

        if (_eastWasPressed)
        {
            SetPressed(AdventureAction.Secondary, false);
        }

        if (_horizontalDirection < 0)
        {
            SetPressed(AdventureAction.Left, false);
        }
        else if (_horizontalDirection > 0)
        {
            SetPressed(AdventureAction.Right, false);
        }

        if (_verticalDirection < 0)
        {
            SetPressed(AdventureAction.Up, false);
        }
        else if (_verticalDirection > 0)
        {
            SetPressed(AdventureAction.Down, false);
        }

        _southWasPressed = false;
        _eastWasPressed = false;
        _horizontalDirection = 0;
        _verticalDirection = 0;
    }

    private static bool Matches(string value, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(value, candidate, StringComparison.OrdinalIgnoreCase));

    public void Dispose()
    {
        _keyboardManager.KeyDown -= OnKeyDown;
        _keyboardManager.KeyUp -= OnKeyUp;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
