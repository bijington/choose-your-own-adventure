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
    private Orbit.Input.GameController? _gameController;
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
            _gameControllerManager.GameControllerConnected += GameControllerManagerOnGameControllerConnected;
            await _gameControllerManager.StartDiscovery();
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

    private void GameControllerManagerOnGameControllerConnected(object? sender, GameControllerConnectedEventArgs e)
    {
        ResetControllerEdges();
        _gameController = e.GameController;
        
        e.GameController.ButtonChanged += GameControllerOnButtonChanged;
        e.GameController.ValueChanged += GameControllerOnValueChanged;
    }

    private void GameControllerOnValueChanged(object? sender, GameControllerValueChangedEventArgs e)
    {
        var gameController = _gameController;

        if (gameController is null)
        {
            return;
        }
        
        if (e.ButtonName == gameController.LeftStick.XAxis.Name)
        {
            var x = e.Value;
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
        }

        if (e.ButtonName == gameController.LeftStick.YAxis.Name)
        {
            var y = e.Value;
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
    }

    private void GameControllerOnButtonChanged(object? sender, GameControllerButtonChangedEventArgs e)
    {
        var gameController = _gameController;
        if (gameController is null)
        {
            return;
        }

        if (e.ButtonName == gameController.South.Name)
        {
            var southPressed = e.IsPressed;
            if (southPressed != _southWasPressed)
            {
                SetPressed(AdventureAction.Primary, southPressed);
                _southWasPressed = southPressed;
            }
        }
        else if (e.ButtonName == gameController.East.Name)
        {
            var eastPressed = e.IsPressed;
            if (eastPressed != _eastWasPressed)
            {
                SetPressed(AdventureAction.Secondary, eastPressed);
                _eastWasPressed = eastPressed;
            }
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
