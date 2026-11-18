# Champion celebrations

Matching eight-frame victory cheers with a crouch, small jump, raised fists and
return to the starting pose. All frames have transparent backgrounds and fixed
canvas sizes; don't trim each frame separately because that would shift the
animation anchor.

| Champion | Sheet | Individual frames |
| --- | --- | --- |
| Racing Driver | `driver_celebrate.png` | `driver_celebrate_00.png` … `driver_celebrate_07.png` |
| Charioteer | `charioteer_celebrate.png` | `charioteer_celebrate_00.png` … `charioteer_celebrate_07.png` |
| Space Pilot | `pilot_celebrate.png` | `pilot_celebrate_00.png` … `pilot_celebrate_07.png` |

Each sheet is **1536 × 1024**, with **4 columns × 2 rows** of **384 × 512** cells.
Read left to right, then top to bottom. Suggested playback is 125 ms per frame
(8 fps), looping. The frames preserve the intentional jump displacement.

The currently installed Orbit.Engine 0.2.2-preview.1 exposes `Sprite` constructors
for a sequence of images or image names. The extracted frame PNGs support that
API directly, without requiring runtime sheet slicing. All PNGs are embedded by
the existing project wildcard.

`ChampionSprites.CreateCelebration(champion)` returns a fresh, automatically
looping Orbit `Sprite`, using cached decoded frame images. A scene can call its
`Update(millisecondsSinceLastUpdate)` method, set its `Bounds`, and call
`Render(canvas, dimensions)`, or add it to its game objects as appropriate.
Orbit renders the image using `Sprite.Bounds`; it does not use the dimensions
argument as the image rectangle.

`OpeningScene` plays the winning champion's celebration after the vote-reveal
suspense pause, including offline local selection. The sprite grows into the
expanded winner card. Ties do not celebrate; reset/reopen clears playback, and
a new reveal starts at frame zero. Other scenes retain their static artwork.

Reproduce the sheets and frame exports from the generated originals on macOS:

```sh
swift -module-cache-path /tmp/champion-swift-cache tools/prepare-celebration-sprites.swift
```

The script removes faint translucent haze, retains the original fixed cell grid,
and also writes temporary GIF previews into `/tmp` for animation review.
