# Choose Your Own Adventure — Orbit talk prototype

This prototype uses `Bijington.Orbit.Engine` with .NET MAUI and keeps the audience's first decision as the visual theme for the talk.

## Current flow

1. Opening / dialogue
2. Choose Your Champion
   - Racing Driver
   - Charioteer
   - Space Pilot
3. Reveal real Orbit `GameObject` code beside the selected champion
4. Enter a themed game-loop scene
5. The champion's top-down vehicle travels around Input → Update → Render → Wait
6. The vehicle pauses at each station, highlights it, and the teaching panel follows the active step
7. Further taps advance from the business-app comparison to firmware superloop and real Orbit concepts

## Game-loop implementation

`GameObjects/LoopVehicle.cs` is deliberately a real Orbit `GameObject`:

- `Update(...)` advances the vehicle between loop stations and adds short teaching pauses.
- `Render(...)` calculates a position and tangent on the ellipse, rotates the vehicle with the track, and renders a car, chariot, or spacecraft based on `AdventureState.Champion`.

The scene is therefore explaining the same loop that is powering its animation.

## Notes

The project file and MAUI registrations are preserved from the uploaded working project. `GameLoopScene` remains registered with DI in `MauiProgram.cs`.

## Input architecture

The presentation now consumes semantic `AdventureAction` values through `AdventureInputRouter` rather than having scenes depend on a particular physical input device. Touch currently feeds the router and the project references `Bijington.Orbit.Input` `0.2.2-preview.1` ready for the controller adapter.

This is deliberate: controller input and, later, SignalR audience input should both produce the same actions (`Primary`, `Left`, `Right`, and so on) so the scenes do not care where an action came from.

The champion screen already supports controller-style focus state: `Left` / `Right` changes the highlighted champion and `Primary` confirms it.

## Orbit.Input controls

The presentation routes touch, keyboard and game-controller input through `AdventureInputRouter`, so scenes respond to semantic actions rather than a particular device.

- Controller South button: continue / confirm
- Controller left stick X axis: previous / next champion
- Controller East button: back (reserved for later navigation)
- Keyboard A/D or Left/Right: previous / next champion
- Keyboard Enter/Space: continue / confirm
- Touch/tap: retained as a fallback

`OrbitInputBridge` starts controller discovery when the game view loads and applies edge detection to the South/East buttons and a dead zone to the left stick. The README for Orbit.Input documents `StartDiscovery()`, `ButtonSouth.Value`, `LeftStick.XAxis.Value`, and keyboard `KeyDown`; the bridge keeps those package-specific details away from the scenes.
