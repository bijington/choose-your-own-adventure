using System.Collections;
using System.Reflection;
using Microsoft.Maui.ApplicationModel;
using Orbit.Input;

namespace ChooseYourOwnAdventure.Input;

/// <summary>
/// Bridges Orbit.Input keyboard and game-controller state into the semantic
/// AdventureAction values used by the presentation.
/// </summary>
public sealed class OrbitInputBridge : IDisposable
{
    private const double StickDeadZone = 0.55d;

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
                var controller = FindConnectedController(_gameControllerManager);
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

    private void PollButtons(object controller)
    {
        var southPressed = ReadButton(controller, "ButtonSouth", "South");
        if (southPressed != _southWasPressed)
        {
            SetPressed(AdventureAction.Primary, southPressed);
            _southWasPressed = southPressed;
        }

        var eastPressed = ReadButton(controller, "ButtonEast", "East");
        if (eastPressed != _eastWasPressed)
        {
            SetPressed(AdventureAction.Secondary, eastPressed);
            _eastWasPressed = eastPressed;
        }
    }

    private void PollLeftStick(object controller)
    {
        var x = ReadAxis(controller, "LeftStick", "XAxis");
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

        var y = ReadAxis(controller, "LeftStick", "YAxis");
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

    private static object? FindConnectedController(object manager)
    {
        foreach (var property in manager.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || property.GetIndexParameters().Length != 0)
            {
                continue;
            }

            object? value;
            try
            {
                value = property.GetValue(manager);
            }
            catch
            {
                continue;
            }

            if (value is null || value is string)
            {
                continue;
            }

            if (LooksLikeGameController(value))
            {
                return value;
            }

            if (value is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is not null && LooksLikeGameController(item))
                    {
                        return item;
                    }
                }
            }
        }

        return null;
    }

    private static bool LooksLikeGameController(object value) =>
        value.GetType().Name.Contains("GameController", StringComparison.OrdinalIgnoreCase)
        && GetPropertyValue(value, "LeftStick") is not null;

    private static bool ReadButton(object controller, params string[] buttonNames)
    {
        foreach (var name in buttonNames)
        {
            var button = GetPropertyValue(controller, name);
            var rawValue = button is null ? null : GetPropertyValue(button, "Value");

            if (rawValue is bool boolean)
            {
                return boolean;
            }

            if (rawValue is IConvertible convertible)
            {
                try
                {
                    return convertible.ToDouble(null) > 0.5d;
                }
                catch
                {
                }
            }
        }

        return false;
    }

    private static double ReadAxis(object controller, string stickName, string axisName)
    {
        var stick = GetPropertyValue(controller, stickName);
        var axis = stick is null ? null : GetPropertyValue(stick, axisName);
        var rawValue = axis is null ? null : GetPropertyValue(axis, "Value");

        if (rawValue is IConvertible convertible)
        {
            try
            {
                return convertible.ToDouble(null);
            }
            catch
            {
            }
        }

        return 0d;
    }

    private static object? GetPropertyValue(object target, string propertyName) =>
        target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(target);

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
