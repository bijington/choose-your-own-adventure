# Choose Your Own Adventure — Talk Guide

> **Target duration:** 30–35 minutes  
> **Core story:** Choose a champion → build it with Orbit → make it move → control it → change the input source → give control to the whole room.

This file is intended to live alongside the code and be updated whenever the presentation flow changes. The authoritative slide order currently lives in `Presentation/PresentationDeck.cs`.

---

## The three things the audience should leave with

1. **Orbit is a lightweight game engine built on .NET MAUI Graphics.** It adds game-oriented concepts such as scenes, game objects, update/render loops, and input without leaving the MAUI ecosystem.
2. **Games continuously process state.** Input affects state, `Update` evolves it, and `Render` displays it — even when nobody has just clicked a button.
3. **The source of input is replaceable.** A controller can drive the game locally; SignalR can let an entire audience become the input source without changing the fundamental game-loop model.

Useful phrases to repeat during the talk:

- **Input affects state. Update evolves state. Render displays state.**
- **The browser sends intent. The server owns the shared state.**
- **The source of the input changed; the game loop didn't.**
- **This is not a screenshot — that is a real Orbit `GameObject`.**
- **No event has happened, but the world is still alive.**

---

# Pre-flight checklist

## Presenter app

- Build/run the Mac Catalyst presenter:

```bash
dotnet build -t:Run -f net10.0-maccatalyst src/ChooseYourOwnAdventure.Presenter/ChooseYourOwnAdventure.csproj
```

- Confirm the controller works if one is connected.
- Confirm keyboard fallback works:
  - `Enter` / `Space` = primary / continue
  - `Shift` = secondary
  - `A` / Left Arrow = left
  - `D` / Right Arrow = right
  - `W` / Up Arrow = up
  - `S` / Down Arrow = down
  - `Escape` / Backspace = back
- Confirm the title artwork renders correctly.
- Confirm Cascadia Code is present at:

```text
src/ChooseYourOwnAdventure.Presenter/Resources/Fonts/CascadiaCode-VariableFont_wght.ttf
```

## Audience web app

For local rehearsal:

```bash
cd src/Adventure.Web
dotnet run --launch-profile http
```

Default local URL:

```text
http://localhost:5187
```

Audience page:

```text
http://localhost:5187/join/ORBIT
```

The presenter defaults to:

```text
BaseUrl:      http://localhost:5187
SessionId:    ORBIT
PresenterKey: orbit-local-presenter
```

For a real event, update `LiveAudienceOptions.BaseUrl` to the deployed Azure URL so the QR code points somewhere audience phones can reach.

## Five-minute rehearsal test

Before presenting, run through this minimum path:

1. Presenter opens on title screen.
2. Audience web page says connected / waiting rather than `Connecting…` forever.
3. Open voting and cast at least one vote for each champion using three browser tabs.
4. Close and reveal the vote.
5. Continue to the audience race.
6. Start the race and confirm tapping each browser moves the team it originally voted for.
7. Confirm the race ends and `A / Enter` opens the podium.
8. Confirm the podium advances to the recap.

### Failure fallback

If SignalR is unavailable during the talk:

- The champion screen falls back to local selection.
- Use left/right to highlight a champion.
- Press primary to confirm it.
- Continue the Orbit/game-loop/controller content normally.
- Skip or verbally describe the audience race if networking cannot be restored.

Do **not** let audience networking block the rest of the talk.

---

# Timing overview

| Approx. time | Scene | Goal |
| --- | --- | --- |
| 0:00–2:00 | Title | Why this talk exists |
| 2:00–3:30 | What is Orbit? | Give Orbit a clear definition |
| 3:30–5:30 | Install/Register + `GameSceneView` + load scene | Show how little setup is required |
| 5:30–9:00 | Choose Champion | Audience makes the first meaningful decision |
| 9:00–11:30 | Add `GameObject` + Champion reveal | Show real Orbit code |
| 11:30–13:00 | Orbit.Input | Introduce local input |
| 13:00–17:30 | Game loop | Explain Input → Update → Render → Wait |
| 17:30–21:30 | Vehicle control | Make the game loop visible with live code |
| 21:30–24:30 | SignalR transition + architecture | Replace local input with audience intent |
| 24:30–29:30 | Audience race | Big interactive payoff |
| 29:30–31:00 | Podium | Celebrate the winner |
| 31:00–33:00 | Recap | Reinforce Orbit + loop + multiplayer |

**Rehearsal target:** finish the planned content around **30–31 minutes**. Audience scanning, laughter, slow voting, network latency, and the race will add time live.

---

# Slide-by-slide walkthrough

## 1. Title — `TitleScene`

### On screen

Vintage choose-your-own-adventure-inspired cover art with the .NET bot holding a controller.

### Purpose

Give yourself time to explain the emotional reason for the talk before showing any code.

### Suggested talking points

- “When I was younger, I loved choose-your-own-adventure books.”
- Talk about reaching the bottom of a page and being responsible for what happened next.
- Connect that to games: the world can react immediately to the player's choices.
- “So today I thought I would make the audience partly responsible for what this talk looks like.”
- Briefly set expectations: this is a real .NET MAUI application, not a normal slide deck.

### Action

`A / Enter` → next slide.

### Transition line

> “Before we start making choices, I should probably explain the thing powering the game we are going to build.”

### If running late

Keep this to 60–90 seconds. Do not cut it entirely; it establishes the whole theme.

---

## 2. What is Orbit? — `OrbitOverviewScene` — page 2

### On screen

```text
.NET MAUI
   ↓
MAUI GRAPHICS
   ↓
ORBIT
   ↓
YOUR GAME
```

### Key message

> **Orbit is a lightweight game engine built on .NET MAUI Graphics that adds game-oriented concepts while keeping you inside the MAUI ecosystem.**

### Talking points

- It is not Unity/Godot replacement positioning.
- It is useful when you want game/game-like experiences in a MAUI application.
- Concepts we will encounter today:
  - scenes
  - game objects
  - update/render loop
  - input
- Point out that MAUI Graphics remains underneath the rendering.

### Action

`A / Enter` → next.

### Transition

> “The nice part is that consuming Orbit still looks like a normal MAUI application.”

---

## 3. Install & Register Orbit — `InstallOrbitScene` — page 3

### On screen

```bash
dotnet add package Bijington.Orbit.Engine
```

and:

```csharp
builder
    .UseMauiApp<App>()
    .UseOrbitEngine();
```

### Talking points

- Orbit arrives as NuGet packages and MAUI builder extensions.
- No special editor or runtime is required.
- This remains a normal MAUI project.
- Keep the installation explanation intentionally short.

### Action

`A / Enter` → next.

### Transition

> “We have registered the engine. Now it needs somewhere inside our MAUI UI to render.”

---

## 4. `GameSceneView` — `GameSceneViewIntroScene` — page 4

### On screen

```xml
<orbit:GameSceneView
    x:Name="GameView"
    HorizontalOptions="Fill"
    VerticalOptions="Fill" />
```

### Talking points

- `GameSceneView` is still a MAUI view in a MAUI page.
- Orbit renders into it.
- This is an important “you haven't left MAUI” moment.

### Action

`A / Enter` → next.

### Transition

> “We've registered Orbit and given it somewhere to draw. The final bootstrap step is to give it a scene.”

---

## 5. Load a Game Scene — `LoadGameSceneIntroScene` — page 5

### On screen

```csharp
gameSceneManager.LoadScene<OpeningScene>(GameView);

gameSceneManager.Start();
```

### Talking points

- A `GameScene` represents the current part/world/screen of the game.
- The presentation itself now uses one scene per major presentation beat.
- The `PresentationDeck` centrally registers and orders those scenes.

### Action

`A / Enter` → champion vote.

### Transition

> “Great. We have a world. But every adventure needs a main character.”

---

## 6. Choose Your Champion — `OpeningScene` — page 6

### On screen

Three champion cards plus QR code:

- **The Driver** — Speed. Precision. Questionable braking.
- **The Charioteer** — Glory. Courage. Undefined behaviour.
- **The Space Pilot** — Exploration. Thrusters. Naturally, Orbit.

### Purpose

First major audience interaction and the choice that themes later examples.

### Talking points before opening voting

- Ask audience to scan the QR code.
- “This isn't just choosing an image. It changes the example we use later and decides which team you race for.”
- Tease the network mechanism but do not explain SignalR yet:
  > “The way your votes are reaching this screen is important. We'll come back to that later.”

### Presenter controls

When SignalR is connected, primary (`A / Enter`) advances the vote lifecycle:

1. **Ready** → open voting
2. **Open** → close voting
3. Wait for the **3-second lock-in countdown**
4. **Closed** → reveal winner
5. **Revealed** → accept winner and continue

`B / Esc` resets voting.

If the result is tied, primary reopens voting.

### Let the moment breathe

The reveal sequence is part of the show:

```text
3… 2… 1…
LOCKING IN
↓
AND THE AUDIENCE CHOSE…
↓
winning champion
```

### Transition

Once the winner is accepted, advance into the dedicated branching-page beat.

> “In a proper choose-your-own-adventure book, this is the point where the book tells us where to go next…”

---

## 7. Turn to Page… — `BranchResultScene` — page 27 / 57 / 87

### On screen

The vote result is repeated clearly, followed by a deliberately non-linear instruction:

```text
THE AUDIENCE HAS CHOSEN…

THE SPACE PILOT

To continue your adventure…
TURN TO PAGE 88
```

The branch page depends on the champion:

- Driver → **page 28**
- Charioteer → **page 58**
- Space Pilot → **page 88**

This is an intentional choose-your-own-adventure gag rather than a literal slide number sequence.

All normal slide footers use a simple book-style `— X —` page marker at the bottom-right rather than a bordered badge.

### Talking points

- Let the room see that their vote has changed the “book”.
- Keep this beat short; it is primarily a visual joke and a thematic payoff.
- Suggested line:
  > “Apparently you have chosen wisely. Turn to page 88.”

### Action

Press `A / Enter` to “turn the page”.

### Transition

> “And on that page, we need to turn our chosen champion into something Orbit can actually render.”

---

## 8. Add a `GameObject` — `AddGameObjectScene` — branch page 28 / 58 / 88

### On screen

The real Orbit shape:

```csharp
public sealed class ChampionGameObject
    : GameObject
{
    public override void Render(
        ICanvas canvas,
        RectF dimensions)
    {
        // draw the champion
    }
}
```

and:

```csharp
Add(new ChampionGameObject(state));
```

### Key message

> “This is not pseudocode. The object on screen is genuinely an Orbit `GameObject`.”

### Talking points

- Scene = part of the world.
- `GameObject` = thing that participates in that world.
- `Render` describes how that thing appears.
- `Add(...)` adds it to the current scene.
- The champion choice lives in shared adventure state and drives which version is rendered.

### Action

`A / Enter` → reveal the character.

### Transition

> “We had a scene, we needed something in it, and now we have a real game object.”

---

## 9. Our Main Character — `ChampionRevealScene` — branch page + 1

### Page numbers

- Driver → page 29
- Charioteer → page 59
- Space Pilot → page 89

### On screen

The chosen champion rendered by the actual `ChampionGameObject`.

### Talking points

- Let the character be visible for a moment rather than immediately advancing.
- Reiterate that it is MAUI Graphics rendering through Orbit.
- “At the moment, though, this is basically a very expensive PowerPoint shape.”

### Action

`A / Enter` → input.

### Transition

> “How do we make it move?”

---

## 10. How Does It Move? — `InputPackageScene` — page 30 / 60 / 90

### On screen

```bash
dotnet add package Bijington.Orbit.Input
```

```csharp
builder
    .UseMauiApp<App>()
    .UseOrbitEngine()
    .UseOrbitGameController()
    .UseOrbitKeyboard();
```

and a small controller-state example.

### Talking points

- `Bijington.Orbit.Input` provides game-controller and keyboard support.
- A game needs a way to capture **player intent**.
- Input can be queried or reacted to.
- Keep this API walkthrough light; the important question is what happens *after* input is received.

### Action

`A / Enter` → game loop.

### Transition

> “We know what the player wants to do. But where does that input get processed, and how does the world keep changing?”

---

## 11. Game Loop — `GameLoopScene` — page 31 / 61 / 91

### On screen

The chosen champion moves through:

```text
INPUT → UPDATE → RENDER → WAIT → repeat
```

The visual theme changes with the champion.

### Core comparison

**Typical business application:**

```text
event → command → state change → UI update → wait
```

**Game:**

```text
input → update → render → wait → repeat
```

### Presenter controls

- `A / Enter` → move champion to the next loop node
- `Left / A-key` → previous loop node
- `Right / D-key` → reveal firmware comparison
- `Right / D-key` again → continue to vehicle control

### Talk through each node

#### INPUT

> “What does the player want to do?”

Examples: controller, keyboard, touch — and later, the network.

#### UPDATE

> “Given the input and the time that passed, what changes in our world?”

Examples: position, velocity, animation, timers, collisions.

#### RENDER

> “Draw the current state of the world.”

Important distinction:

> “Rendering ideally does not decide what happened; it displays the state that already says what happened.”

#### WAIT

> “Yield until the next frame, then do the whole thing again.”

Repeat the central line:

> **Input affects state. Update evolves state. Render displays state.**

### Firmware comparison

Keep this to roughly 30–60 seconds:

```c
while (true)
{
    ReadInputs();
    UpdateState();
    WriteOutputs();
    Delay();
}
```

versus:

```csharp
while (running)
{
    Input();
    Update();
    Render();
    Wait();
}
```

Suggested line:

> “Different domain. Remarkably similar mental model.”

### Transition

> “That is the theory. Let's actually drive something through the loop.”

---

## 12. Now Drive It — `VehicleControlScene` — page 32 / 62 / 92

### Purpose

This is the main local-input technical demo. The code on the left changes emphasis while the game on the right reacts to the controller.

### General message

> “Controller input changes state; `Update` evolves the state; `Render` shows the result.”

### Driver path

Concepts:

- throttle
- steering
- velocity
- multiple simultaneous inputs

Useful commentary:

> “Throttle changes our speed while steering changes a different piece of state during the same update.”

### Charioteer path

Concepts:

- force
- momentum / inertia
- low drag

Useful commentary:

> “I have released the input, but the chariot is still moving. State accumulated in previous frames still matters.”

### Space Pilot path

Concepts:

- thrust while button is held
- gravity when released
- velocity and position evolving continuously

Useful commentary:

> “I'm pressing absolutely nothing now, but the game hasn't stopped. Gravity is still changing velocity, Update is still changing position, and Render is still showing it.”

Then:

> **“No event has happened, but the world is still alive.”**

### Moving on

Current code uses `Up / W` to leave the interactive vehicle scene and move to the SignalR transition. Touch also has a lower-right next area.

### Transition

> “This works nicely, but I have one controller… and there are rather more of you.”

---

## 13. What If the Input Source Changes? — `SignalRTransitionScene` — page 33 / 63 / 93

### On screen

Conceptual transition from local controller intent to remote/audience intent.

### Talking points

- The downstream game logic should not care whether intent came from a physical controller or a network message.
- Hold up the controller if useful.
- Ask:
  > “What if, instead of this controller, the input came from all of you?”

### Action

`A / Enter` → architecture reveal.

### Transition

> “The interesting thing is: you have already been using the architecture that lets us do that.”

---

## 14. SignalR: The Room Becomes the Input — `SignalRArchitectureScene` — page 34 / 64 / 94

### On screen

```text
AUDIENCE BROWSERS
      ↓
 SIGNALR HUB
      ↓
SERVER-OWNED STATE
      ↓
  MAUI + ORBIT
```

### Reveal

> “The champion vote at the start was already using SignalR.”

### Talking points

Keep this to four ideas:

1. Audience joins through a browser; no app installation required.
2. The browser sends **intent** through SignalR.
3. The server owns/validates the shared state.
4. The MAUI presenter receives that state and Orbit renders it.

Key phrase:

> **“The browser does not directly move a vehicle. It says: I want to boost my team.”**

Mention batching briefly:

- taps feel immediate in the browser
- browser batches taps into fewer SignalR calls
- avoids one network message per physical tap

### Action

`A / Enter` → audience race.

### Transition

> “And because the server remembered who each of you voted for earlier, that first decision is about to matter again.”

---

## 15. Audience Grand Prix — `AudienceRaceScene` — page 35 / 65 / 95

### On screen

All three champions race simultaneously. Audience phones automatically become team-specific boost controls.

### Before starting

Say:

> “You chose your champion earlier. You have been on that team ever since.”

Explain fairness in one sentence:

> “Boost strength is normalised by team size, so having more people does not automatically guarantee a win.”

### Presenter controls

- `A / Enter` while race is **Ready** → start race
- `B / Esc` → reset race
- `A / Enter` after **Finished** → podium

### During the race

Do not over-explain. Let the room play.

Aim for **60–90 seconds**.

Optional commentary:

- “Driver team are awake.”
- “Space Pilot appears to have discovered horizontal scaling.”
- “Charioteers, I admire your commitment to eventual consistency.”

### Technical payoff after/during finish

> “The phone did not move that vehicle directly. It sent intent, the server changed the authoritative race state, and Orbit rendered the result.”

### Transition

After the race finishes, press `A / Enter`.

> “Every adventure deserves an ending… and every race deserves a podium.”

---

## 16. The Podium — `PodiumScene` — page 36 / 66 / 96

### On screen

1st / 2nd / 3rd podium generated from the final race state, including all three champion vehicles and team sizes.

### Purpose

Celebrate the audience before switching back into recap mode.

### Talking points

- Call out the winning team.
- Thank the other teams.
- Keep this short and energetic.
- This is a good place to let applause/reaction happen without talking over it.

### Action

`A / Enter` → recap.

### Transition

> “So what did we actually build along the way?”

---

## 17. Adventure Complete — `RecapScene` — page 37 / 67 / 97

### On screen

Three summary cards:

**ORBIT**

- Scenes
- GameObjects
- Update + Render

**ORBIT.INPUT**

- Controller
- Keyboard
- Player intent

**SIGNALR**

- Audience input
- Shared state
- Real-time multiplayer

### Recap script

> “We started with a normal .NET MAUI application.”

> “Orbit gave us scenes, game objects, and a game loop.”

> “Orbit.Input let us control that world locally.”

> “Then SignalR let us replace one local input source with an entire room.”

Repeat:

> **“The source of the input changed; the game loop didn't.”**

Then:

> “The biggest takeaway isn't that every MAUI app needs to become a video game — although obviously it should. It is that games use techniques like immediate feedback, continuous state, animation, and shared experiences to make software feel alive.”

Suggested final line:

> **“Thank you for choosing your own adventure.”**

---

# Page-number map

Before branching, the page is the slide's explicit sequential value (1–6). From `BranchResult` (slide 7), add the selected champion's enum value: Driver 20, Charioteer 50, Space Pilot 80. The branch result points to `AddGameObject` (slide 8), so its “TURN TO PAGE” exactly matches the next footer.

| Beat | Driver | Charioteer | Space Pilot |
| --- | ---: | ---: | ---: |
| Branch result | 27 | 57 | 87 |
| Add GameObject | 28 | 58 | 88 |
| Champion reveal | 29 | 59 | 89 |
| Input | 30 | 60 | 90 |
| Game loop | 31 | 61 | 91 |
| Vehicle control | 32 | 62 | 92 |
| SignalR transition | 33 | 63 | 93 |
| SignalR architecture | 34 | 64 | 94 |
| Audience race | 35 | 65 | 95 |
| Podium | 36 | 66 | 96 |
| Recap | 37 | 67 | 97 |

The branch offset remains in effect through the recap. The presenter and web enum values and browser mapping must agree.

---

# Input cheat sheet

## Ordinary presentation scenes

| Intent | Controller | Keyboard | Touch |
| --- | --- | --- | --- |
| Continue | South / A | Enter / Space | Tap |
| Back | East / B | Escape / Backspace | — |

## Directional input

| Intent | Controller | Keyboard |
| --- | --- | --- |
| Left | Left stick left | A / Left Arrow |
| Right | Left stick right | D / Right Arrow |
| Up | Left stick up | W / Up Arrow |
| Down | Left stick down | S / Down Arrow |
| Secondary | East / B | Shift |

## Special scenes

### Champion vote

- Primary = open / close / reveal / accept
- Left/right = locally highlight champion
- Secondary/back = reset vote when networked

### Game loop

- Primary = advance node
- Left = previous node
- Right = firmware comparison / then continue to vehicle scene

### Vehicle control

- Uses held input for the live movement demo
- `Up / W` = continue to SignalR section

### Audience race

- Primary = start race / continue after finish
- Secondary/back = reset race

---

# Running-late plan

If the clock is slipping, cut/compress in this order:

1. **Firmware comparison** — reduce to one sentence and the two loops.
2. **Orbit installation detail** — show the code, say “NuGet + builder extension,” move on.
3. **Input package API explanation** — state what it provides; do not enumerate APIs.
4. **SignalR implementation detail** — explain the architecture diagram, not hub code.
5. **Vehicle demo repetitions** — demonstrate the chosen mechanic once clearly.
6. **Race** — keep it to one quick heat; do not restart.
7. **Podium** — celebrate winner in 15–20 seconds and move to recap.

Do **not** cut:

- the opening story entirely
- the champion vote (if networking works)
- the game-loop explanation
- the local-input → SignalR-input transition
- the final recap

Those are the narrative spine.

---

# If you have extra time

Use extra minutes to let existing moments breathe rather than introducing another topic:

- spend a little more time on why Orbit exists
- explain the chosen vehicle physics more deeply
- show one small SignalR hub/client code fragment
- talk about server-authoritative state and tap batching
- let the audience enjoy the vote/race reactions

Avoid adding another major feature to the talk.

---

# Maintenance notes

When changing the presentation, update this guide at the same time as:

- `Presentation/PresentationDeck.cs` — slide order and scene registration
- `PresentationSlide.cs` — slide identifiers
- `MainPage.xaml.cs` — special-scene controls/transitions
- `Presentation/PresentationTheme.cs` — page numbers / branch page scheme / styling

If a scene becomes interactive in a special way, add its controls to both **Input cheat sheet** and its scene section above.

