# Choose Your Own Adventure — themed game-loop milestone

This milestone turns the audience's first decision into the visual theme for the session.

## Flow

1. Opening/title sequence.
2. **Choose Your Champion**:
   - The Racing Driver
   - The Charioteer
   - The Space Pilot
3. The selected champion is stored in `AdventureState`.
4. A real Orbit `ChampionGameObject : GameObject` renders the chosen champion.
5. The next scene becomes a themed game-loop visual:
   - Driver → The Looping Speedway
   - Charioteer → The Looping Arena
   - Space Pilot → The Orbital Loop
6. The racer itself is a real Orbit `LoopRacer : GameObject` and moves from `Update(...)`.
7. Tapping through the game-loop scene moves through:
   - business app vs game
   - firmware superloop vs game loop
   - Orbit `Update` vs `Render`

## Teaching point

The racetrack is not a pre-baked animation. `LoopRacer.Update(...)` advances progress using elapsed time and `LoopRacer.Render(...)` draws the current state, so the visual demonstrates the game loop by actually using it.

## Next likely step

Replace tap navigation with the Orbit controller-support package, then add speaker-safe next/back/checkpoint commands before introducing SignalR audience controls.
