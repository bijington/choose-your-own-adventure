# Choose Your Own Adventure — Orbit vertical slice

This starter targets .NET 10 and includes Android, iOS, Mac Catalyst, and Windows (when building on Windows).

## Current vertical slice

1. Orbit title screen with animated `PRESS START`.
2. Tap/click to advance the opening dialogue.
3. Choose one of three classes by tapping/clicking its third of the screen.
4. The choice is stored in the singleton `AdventureState`.
5. Tap again to reveal a split-screen teaching moment:
   - C# representing the selected class on the left.
   - A simple live-rendered pixel character on the right.

The sprite is intentionally rendered with MAUI Graphics primitives rather than an image asset. That keeps the demo honest: the right-hand side is being rendered live by the Orbit scene.

## Mac Catalyst

Clean and run with:

```bash
rm -rf bin obj
dotnet restore
dotnet build -t:Run -f net10.0-maccatalyst
```

The project contains the Mac Catalyst bundle-name workaround required by the current .NET SDK behavior seen during development.

## Input

For now, mouse/touch input advances the scene and chooses a class. This is deliberately isolated from the scene state so Orbit controller support can be added next without rewriting the story/rendering code.

## Real Orbit code reveal

The class-selection payoff now uses a real Orbit `GameObject` rather than a presentation-only `hero.Render(...)` example.

`OpeningScene` adds the character using Orbit's real scene API:

```csharp
Add(new Adventurer(state));
```

`Adventurer` derives from `GameObject` and overrides Orbit's actual render hook:

```csharp
public override void Render(ICanvas canvas, RectF dimensions)
```

During the reveal, the code panel on the left is a curated view of the real implementation in `GameObjects/Adventurer.cs`, while that same `Adventurer` instance renders the selected class on the right.
