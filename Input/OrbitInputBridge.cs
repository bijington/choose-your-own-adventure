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
            // Expected when shutting down.
        }
        catch (Exception)
        {
            // Controller support is optional. Keyboard/touch must remain usable
            // even if platform discovery is unavailable or fails.
        }
    }

    private void OnKeyDown(object? sender, KeyboardKeyChangeEventArgs key)
    {
        // Comparing names rather than individual enum members keeps this adapter
        // useful across keyboard layouts while Orbit.Input does the platform work.
        var name = key.ToString();

        if (Matches(name, "KeyA", "ArrowLeft", "LeftArrow", "Left"))
        {
            Trigger(AdventureAction.Left);
        }
        else if (Matches(name, "KeyD", "ArrowRight", "RightArrow", "Right"))
        {
            Trigger(AdventureAction.Right);
        }
        else if (Matches(name, "KeyW", "ArrowUp", "UpArrow", "Up"))
        {
            Trigger(AdventureAction.Up);
        }
        else if (Matches(name, "KeyS", "ArrowDown", "DownArrow", "Down"))
        {
            Trigger(AdventureAction.Down);
        }
        else if (Matches(name, "Enter", "Return", "Space", "Spacebar"))
        {
            Trigger(AdventureAction.Primary);
        }
        else if (Matches(name, "Escape", "Backspace"))
        {
            Trigger(AdventureAction.Back);
        }
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
            // Expected when the page/app shuts down.
        }
    }

    private void PollButtons(object controller)
    {
        var southPressed = ReadButton(controller, "ButtonSouth", "South");
        if (southPressed && !_southWasPressed)
        {
            Trigger(AdventureAction.Primary);
        }
        _southWasPressed = southPressed;

        var eastPressed = ReadButton(controller, "ButtonEast", "East");
        if (eastPressed && !_eastWasPressed)
        {
            Trigger(AdventureAction.Back);
        }
        _eastWasPressed = eastPressed;
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

        if (horizontalDirection != 0 && horizontalDirection != _horizontalDirection)
        {
            Trigger(horizontalDirection < 0 ? AdventureAction.Left : AdventureAction.Right);
        }
        _horizontalDirection = horizontalDirection;

        var y = ReadAxis(controller, "LeftStick", "YAxis");
        var verticalDirection = y switch
        {
            < -StickDeadZone => -1,
            > StickDeadZone => 1,
            _ => 0
        };

        if (verticalDirection != 0 && verticalDirection != _verticalDirection)
        {
            Trigger(verticalDirection < 0 ? AdventureAction.Up : AdventureAction.Down);
        }
        _verticalDirection = verticalDirection;
    }

    private void Trigger(AdventureAction action)
    {
        MainThread.BeginInvokeOnMainThread(() => _router.Trigger(action));
    }

    private void ResetControllerEdges()
    {
        _southWasPressed = false;
        _eastWasPressed = false;
        _horizontalDirection = 0;
        _verticalDirection = 0;
    }

    private static object? FindConnectedController(object manager)
    {
        // The README documents discovery and the GameController surface but does not
        // name the manager member that exposes discovered controllers. Keep that tiny
        // package-version detail isolated here while the rest of the app remains strongly
        // typed against Orbit.Input.
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
                    // Try the next documented alias.
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
                return 0d;
            }
        }

        return 0d;
    }

    private static object? GetPropertyValue(object target, string propertyName) =>
        target.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
            ?.GetValue(target);

    private static bool Matches(string value, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(value, candidate, StringComparison.OrdinalIgnoreCase));

    public void Dispose()
    {
        _keyboardManager.KeyDown -= OnKeyDown;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
