# Choose Your Own Adventure — controller-driven game loop

This iteration adds two presentation beats:

1. **Walk the game loop** — the chosen champion moves between INPUT, UPDATE, RENDER and WAIT one node at a time when the primary controller button is pressed. Right advances first to the firmware comparison and then to the driving scene.
2. **Drive the loop** — the champion enters a themed vehicle. Hold the South/A button (or Enter/Space) to accelerate and East/B (or Shift) to brake. The real code panel on the left becomes bold/bright when its Input, Update and Render sections are active.

The input router now supports both edge-triggered actions and held state. This is intentionally useful for later mechanics such as a rocket that rises while a button is held and falls under gravity when released.

## Controls

- South / A / Enter / Space: primary action; hold to accelerate in the vehicle scene
- East / B / Shift: brake in the vehicle scene
- Left stick / A-D / arrow keys: navigation
- Touch: still works as a fallback; in the vehicle scene a tap gives a short acceleration pulse
