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

# Presenter notes window

The desktop presenter opens two windows: **Audience** (the book) and **Presenter Notes** (private cues). Keep the book on the projector and notes on the laptop using an extended desktop. Move the windows before the session, then rehearse full-screen behavior on the actual display setup.

If notes are closed, use **Presenter → Show Presenter Notes**. **Return to presentation** changes focus without advancing a page. Closing/reopening notes preserves the session clock and current scene. The notes window follows rendering reveals, loop stations/firmware, vote phases/ties and race completion, with the current controls and next action always visible.

Speaker prose is extracted from this guide at build time; code blocks and audience-screen descriptions are omitted. Interactive cues are maintained in `PresenterNotes/PresenterCue.cs`. Rebuild after editing this guide.

# Fresh audience sessions

Before the audience joins, choose **Presenter → New Audience Session** in either desktop window (or **New audience session** in the mobile options menu). This creates a unique session ID and updates the audience QR code/link without server changes. Old test votes stay in the old session. Ask everyone to scan the new code; people already in the old session will not move automatically. The session is remembered across restarts on this device. Practice mode stays enabled if already on, so switch to live audience when ready.

# Mobile rehearsal

Phones and tablets default to Practice mode. Use the **Practice · options** button to choose a local champion, skip the race or jump to a slide. Card taps select the champion locally; the race's right page skips to the epilogue. No SignalR connection or presenter commands are used in practice. The mode is remembered on the device. Select **Use live audience** to reconnect and rehearse with real phones. Desktop offers **Presenter → Practice Options** as well.

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

Optional aside **only if setup or connectivity misbehaves**: “There were going to be goblins. This one lives in the conference Wi-Fi.” Keep this in reserve; there is no planned goblin reveal.

For connection recovery, the client reconnects, rejoins the session and receives the latest vote/race snapshot. Keep the same browser identity to retain the team. This restores current state, not every missed tap. Use the reconnect/QR flow or practice mode to keep moving.

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
| 2:00–3:30 | Building a Game in MAUI | From MAUI to Graphics, Orbit and our game |
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

## Title — `TitleScene`

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

## Opening choice — `FirstDecisionScene` — page 1

Allow about 30 seconds after the cover. Invite a show of hands or a shouted answer, without opening SignalR voting:

> “One last session… or straight to the after-party?”

- **Attend the talk → page 2**: A/Enter, or tap the upper right-page card.
- **Go straight to the after-party → page 404**: B/Shift, or tap the lower card.
- Left page / Escape: return to the cover.

The optional page-404 spread reads:

> “The after-party could not be found. Please complete this adventure first.”

Pause for the joke, then A/Enter or tap the right page to join the prologue at page 2. The normal talk route skips page 404, and Back from the prologue returns to the decision page. This introduces the book format before the real champion vote.

## Book chapters

Every section has a dedicated right-page opening and matching running headers. Each chapter leaf leaves the left page blank.

| Section | Content |
| --- | --- |
| Prologue · Discovering Orbit | Orbit overview through loading a scene |
| Chapter One · Meet Our Champion | Champion vote, GameObject, character reveal |
| Chapter Two · Take Control | Input package and local driving/flying |
| Chapter Three · An Adventure Together | SignalR explanation and audience race |
| Epilogue · The Journey Home | Podium and recap |

## PROLOGUE · DISCOVERING ORBIT — `PrologueChapterScene` — page 2

Right-page chapter opener. Left page blank.

> Before we choose a path, let’s discover the engine that brings this world to life.

A/Enter or right-page tap continues; left-page tap goes back.

## MAUI Already Has Some Magic — `MauiAnimationsScene`

Before the Orbit overview, spend about 30 seconds showing three real MAUI animations: a bouncing control, a spinning/scaling star and a pulsing heart. Tap **Play it again** to restart or **Build a game →** to continue. The examples run automatically and stop when leaving the spread.

> “Before we reach for a game engine, MAUI already gives us a little magic. These are ordinary XAML controls. We can move, rotate, scale and fade them, and easing gives that movement personality.”

> “That is great for playful UI and feedback. As our game grows, we want a shared game loop, scenes and game objects. That is where Orbit comes in.”

The left page shows the same MAUI Animation approach used by the live controls. Continue with South / Enter / right-page tap / right shoulder; left-page tap goes back. Leave space around the controls to tap the page, or use keyboard/controller navigation.

---

## Building a Game in MAUI — `OrbitOverviewScene` — page 3

### On screen

The same book spread has four talking points. The right page always shows `.NET MAUI → MAUI GRAPHICS → ORBIT → YOUR GAME`, highlighting one box at a time. The left page explains that building block and ends with one takeaway. The Orbit logo is hidden for MAUI and MAUI Graphics, appears when Orbit is introduced at step three, and stays visible for Your Game.

### Talking points

1. **.NET MAUI — Start with a MAUI app.** MAUI supplies the application, windows, pages and controls. The audience game view and the XAML presenter notes share this host. First ask: how could we make a game here?
2. **MAUI Graphics — Give the world a canvas.** `ICanvas` draws shapes, text and images. We can build games directly with those tools, but still need to organize the world and arrange state updates and drawing. Graphics supplies the drawing tools.
3. **Orbit — Add structure and a loop.** Scenes organize the world and GameObjects give its parts behaviour. The engine runs Update/Render; MAUI Graphics stays underneath. The separate Orbit.Input package comes later in the controls chapter.
4. **Your Game — Write the rules.** Our code chooses the characters, state and response to input: steering, momentum or gravity. MAUI hosts it; Graphics draws; Orbit supplies structure and the loop; our code defines the experience.

### Action

South / Right shoulder / Enter / Page Down / right-page tap advances one building block. Left shoulder / East / Page Up / Escape / left-page tap goes back one step; from step 1 it returns to the MAUI animation demo. Only after Your Game does the next action open Install & Register Orbit. Re-entering the spread starts again at .NET MAUI. Presenter notes follow each step.

### Transition

> “Now let's see how little setup it takes to add Orbit to that MAUI app.”

---

## Install & Register Orbit — `InstallOrbitScene` — page 4

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

## `GameSceneView` — `GameSceneViewIntroScene` — page 5

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

## Load a Game Scene — `LoadGameSceneIntroScene` — page 6

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

## Chapter One opening — `ChampionChapterScene` — page 7

Introduce “MEET OUR CHAMPION” before voting. The right page contains the chapter title and self-discovery lead-in; the left stays blank. No selected character appears yet. Continue opens the champion vote; Back returns to the end of the prologue.

## Choose Your Champion — `OpeningScene` — page 8

### On screen

Voting instructions, QR code and live status on the left page. Three stacked champion cards on the right show their destination pages:

- **The Driver** — page 29 — Speed. Precision. Questionable braking.
- **The Charioteer** — page 59 — Glory. Courage. Undefined behaviour.
- **The Space Pilot** — page 89 — Exploration. Thrusters. Naturally, Orbit.

### Purpose

First major audience interaction: the winning champion selects the movement model explored later, and each participant’s vote determines their race team.

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

`B / Shift` resets voting. Escape or a left-page tap goes back.

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

Celebrate the enlarged winner on the selection spread, then turn to the chosen-champion summary.

> “In a proper choose-your-own-adventure book, this is the point where the book tells us where to go next…”

---

The final result stays on this spread: the losing cards fade out while the winning card enlarges on the right page. Its final vote count and “TURN TO PAGE” destination stay visible. Once the reveal finishes, press A/Enter or tap the right page to turn to the chosen-champion summary. Left-page taps or Escape return to Load a Game Scene; B/Shift resets voting. A tie retains all choices and can reopen voting.

---


## Our champion’s origins — `ChosenChampionScene` — page 29 / 59 / 89

### Talking points

Introduce the champion on the right-hand summary page first. Point to the consequence on the facing page:

> “You’ve chosen more than our hero — you’ve chosen the physics we’ll have to live with.”

Name the chosen path: **Driver — precision, steering and braking; Charioteer — building and taming momentum; Space Pilot — thrust against gravity.** Return to this consequence in the control demo: “This is the challenge you chose for us.” The choice changes the working example and the controls, while everyone’s original vote keeps their race team.

Then invite the audience into their backstory: “Every champion has an origin story. Ours begins with a dot.” Trace their origins in four local stages:

1. **Dot:** a filled ellipse. Every champion starts somewhere; a canvas and bounds are enough to show something.
2. **Primitives:** combine circles, rectangles and lines into a simple figure. More handmade detail needs more instructions. The chosen champion influences the colour.
3. **Image:** show the current static character PNG. Decode/cache it once, preserve its aspect ratio, and draw it with DrawImage through the existing helper. The asset carries the detail, so the rendering call stays small.
4. **Animated Sprite:** play the real eight-frame Orbit celebration. Create it once, call Update with elapsed milliseconds and Render into its Bounds. The engine advances the frames. We are animating a sprite here; sprites do not have to be animated by definition.

The Render canvas-and-bounds contract is displayed throughout. Say: **“The artwork gets richer. The rendering contract stays familiar.”** Primitive detail adds drawing instructions; image assets and the Sprite abstraction move that complexity out of the scene's drawing code.

### Action

South / Enter / Right shoulder / Page Down / right-page tap opens the dot, then advances through primitives, image and sprite. Only the next action after the sprite opens the champion's game-loop story. Left shoulder / East / Page Up / Escape / left-page tap steps backwards: sprite → image → primitives → dot → summary → vote. Re-entering the spread starts at the summary.

### Transition

> “We've given them more detail, and now a little life. How does their world keep changing from one frame to the next?”

The four stages share one printed page. The celebration uses the existing cached frames and each scene's own Orbit Sprite playback state; it does not start another game loop.

---

## A Frame in the Life of Our Champion — `GameLoopScene` — page 30 / 60 / 90

### Purpose

Finish meeting the champion by discovering how their world stays alive. The audience has already seen the character and their Render code; now explain the rhythm that keeps that character changing. Teach intent conceptually before wiring up controller input in Chapter Two.

### On screen

Start with **A Familiar Rhythm**: firmware superloop and conceptual game-loop code on the facing pages. Establish read → update → output → repeat first. Then apply the same idea to the selected champion, walking them through INPUT → UPDATE → RENDER → WAIT.

### Firmware comparison — first

The spread opens on the familiar firmware superloop alongside conceptual game-loop pseudocode. Keep the analogy brief; Orbit owns the actual loop. South / Enter / Right shoulder / Page Down / right-page tap shows the champion walkthrough. Back from this first comparison returns to the character summary.

### Talking points — then the champion

The left page pairs a champion thought with the current engine responsibility. The right page walks the selected character around the loop. No page turn occurs between these beats.

1. **Input — ask what the player wants.** Imagine the intent before showing the input package. The pilot asks, “Thrusters, please. Preferably now.”
2. **Update — evolve the world.** Use elapsed time. The pilot says, “No thrust? Gravity has opinions.” The charioteer notices momentum: “The reins are loose. I'm still rolling.”
3. **Render — make the result visible.** Connect to the rendering evolution we just saw. Rendering displays the state that Update produced.
4. **Wait — do it again.** The engine yields until the next frame. This is a teaching step-through; Orbit runs the real loop continuously.

Repeat: **Input supplies intent. Update evolves state. Render makes state visible.**

### Action

On the champion walkthrough, South / Enter / right-page tap advances stations; East / Escape / Left / left-page tap reverses a station, returning to firmware before Input. Left shoulder / Page Up returns directly to the firmware comparison, preserving the selected station. Right shoulder / Page Down or the lower-right NEXT area opens Chapter Two: Take Control. Re-entering this spread starts with firmware and resets the station to Input.

### Transition

> “Now we know how their world runs. Let's put the controls in your hands.”

---

## CHAPTER TWO · TAKE CONTROL — `InputChapterScene` — page 31 / 61 / 91

Right-page chapter opener. Left page blank.

> We know how their world runs. Now let’s put the controls in your hands.

A/Enter or right-page tap continues; left-page tap goes back.

## MAUI Already Has Input — `MauiInputScene` — page 32 / 62 / 92

### On screen

An ordinary XAML Button uses Pressed and Released. The facing page shows handlers that set/clear intent on our AdventureInputRouter. These are teaching excerpts for a game UI, not live buttons on this spread.

### Talking points

- We can already use normal MAUI controls with touch screens and a mouse.
- Clicked or Command can represent an action; Pressed/Released can represent a held control.
- The event handler sets intent. It does not directly move or draw the champion. Update evolves state and Render draws it.
- Gestures and pointer events provide other interaction options.
- Orbit.Input adds keyboard and game-controller support; it complements the MAUI UI we already have.

### Action

South / Enter / Right shoulder / Page Down / right-page tap opens the Orbit.Input package spread. Back returns to the Take Control chapter opener.

### Transition

> “Touch and mouse are already covered. What if we want a keyboard or a game controller?”

The [MAUI Button documentation](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/button?view=net-maui-10.0) describes touch/mouse activation and Pressed/Released behavior.

---

## How Does It Move? — `InputPackageScene` — page 33 / 63 / 93

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

`South / Enter` → live vehicle control.

### Transition

> “We have seen the loop. Now watch real input change our champion’s state.”

---

## Now Drive It — `VehicleControlScene` — page 34 / 64 / 94

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

Current code uses `Up / W` to leave the interactive vehicle scene and move to the SignalR chapter opening. Touch also has a lower-right next area.

### Transition

> “This works nicely, but I have one controller… and there are rather more of you.”

---

## CHAPTER THREE · AN ADVENTURE TOGETHER — `SignalRChapterScene` — page 35 / 65 / 95

Right-page chapter opener. Left page blank.

> One champion. An entire room of players. Let’s turn the audience into the input.

A/Enter or right-page tap continues; left-page tap goes back.

## What If the Input Source Changes? — `SignalRTransitionScene` — page 36 / 66 / 96

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

## SignalR: The Room Becomes the Input — `SignalRArchitectureScene` — page 37 / 67 / 97

### On screen

One spread follows **one champion vote in four stages**. The left page shows focused code excerpts from this application; the right page keeps the complete route visible and highlights the component being explained. The printed page number stays the same throughout.

### Talking points

Start with the concept before following the code (about 30 seconds):

> “SignalR is ASP.NET Core’s library for real-time communication between a server and connected clients. Normally a browser asks for something and the server replies. Here, the server can also push an update when something changes, without every phone repeatedly asking whether there is news.”

> “Think of our hub as the room’s switchboard. Your phone sends ‘I choose this champion’. The server records that choice, then announces the updated result to the connected clients in our session — including this MAUI app. The phones and the presentation are clients of the same server.”

Keep the responsibilities clear: **SignalR carries messages; our application decides what those messages mean.** The hub routes calls, our vote/race stores own the shared state, and Orbit renders the latest state. SignalR is useful for live dashboards, chat and collaborative experiences as well as audience games. It does not replace the game loop.

Then say: **“Let’s follow one vote all the way from your phone to this screen.”**

1. **Audience browser:** JavaScript builds the connection to `/adventureHub`, starts it, joins the session and invokes `Vote` with the session, participant and selected champion. The connection setup and card-click call are shown together. The phone sends a choice; it does not calculate a winner.
2. **ASP.NET Core hub:** Match `/adventureHub` to `MapHub<AdventureHub>` and `'Vote'` to `AdventureHub.Vote(...)`. Arguments arrive in order. Mention the omitted validation/normalization and that `JoinSession` puts each connection into the session group.
3. **Server state and broadcast:** `VoteStore.CastVote` accepts votes while open, stores one choice per participant under a lock, and returns a snapshot. `Clients.Group(normalized).SendAsync("VoteUpdated", snapshot)` pushes it to the presenter and audience browsers in this session. A hub routes calls; the store owns the shared state.
4. **MAUI client and Orbit:** `_connection.On<VoteSnapshot>("VoteUpdated", _voteState.Update)` receives the same event name. The callback updates shared state under a lock; `OpeningScene` reads it during the existing render loop. SignalR changes state and Orbit draws it. No extra rendering loop is needed.

The presenter window follows each stage with its own talking points. The code is a teaching excerpt: validation, retry handling and surrounding methods are intentionally omitted where marked.

### Action

South / Right shoulder / Enter / Page Down / right-page tap steps forward. After stage 4, the next action opens the race. Left shoulder / East / Page Up / Escape / left-page tap steps backward; from stage 1 it returns to the preceding spread. Entering this spread again starts at stage 1.

### Transition

> “The race follows exactly the same route: Boost intent goes in, RaceUpdated state comes out. And the server remembered which team each of you chose.”

Briefly mention that the browser batches boost taps into fewer calls. Then give control to the room.

---

## Audience Grand Prix — `AudienceRaceScene` — page 38 / 68 / 98

### On screen

All three champions race simultaneously. Audience phones automatically become team-specific boost controls. Moving vehicles use the same animated sprites as the control demo: rolling tyres, chariot wheels and dust, and rocket exhaust. Each has independent playback and returns to the static artwork while idle or after the finish.

### Before starting

Say:

> “This is our final challenge. You chose your champion earlier, and you have been on that team ever since. Now power them through the last stretch.”

Before starting, expose the design bug (about 20 seconds):

> “What happens if one champion has twice as many people? Have we built a race — or a headcount?”

Pause for an answer. Explain that raw tap totals would reward team size. The fix is already in our server: each tap advances progress by `1 / (team size × 84)`, so average effort per registered team member determines progress. More people do not automatically guarantee a win; inactive team members still count. This is a design challenge we have solved, not a staged live failure.

> “We divide tap strength by team size. A small party can still win this adventure. Ready?”

### Presenter controls

- `A / Enter` while race is **Ready** → start race
- `B / Esc` → reset race
- `A / Enter` after **Finished** → epilogue chapter, then podium

### During the race

Do not over-explain. Let the room play.

Allow **60–90 seconds** for instructions, the race and the reaction. The race itself targets about **28 seconds** at three taps per second per person: 84 taps per registered team member, or 1,680 taps for a team of 20. This is an effort target, not a timer; faster tapping finishes sooner, and inactive team members slow their team down.

Optional commentary:

- “Driver team are awake.”
- “Space Pilot appears to have discovered horizontal scaling.”
- “Charioteers, I admire your commitment to eventual consistency.”

### Technical payoff after/during finish

> “The phone did not move that vehicle directly. It sent intent, the server changed the authoritative race state, and Orbit rendered the result.”

### Transition

After the race finishes, press `A / Enter`.

> “You chose the heroes, shaped their journey and powered the final challenge. The room wrote the ending — now let’s see our podium.”

---

## EPILOGUE · THE JOURNEY HOME — `EpilogueChapterScene` — page 39 / 69 / 99

Right-page chapter opener. Left page blank.

> Every race has a finish. Every adventure leaves us with something to take home.

A/Enter or right-page tap continues; left-page tap goes back.

## The Podium — `PodiumScene` — page 40 / 70 / 100

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

## Adventure Complete — `RecapScene` — page 41 / 71 / 101

### On screen

The closing spread thanks the audience and names **Shaun Lawrence**. The left page recaps three steps: start with MAUI, bring it to life with Orbit and input, and let the room play with SignalR. The right page offers the blog post and resources, with a QR code and the URL beneath it.

Configure `TalkSummary:ResourcesUrl` before the talk. An empty or invalid URL shows a “Resources coming soon” panel rather than a QR code. This link is separate from the live audience join URL. No credentials belong in the resources URL.

### Talking points

- “We started with a normal MAUI application and a canvas.”
- “Orbit gave us scenes, game objects and the Update/Render loop. Input let us control that world.”
- “SignalR let the entire room become the input source.”
- **“The input source changed. The game loop stayed the same.”**
- When the blog URL is configured, invite the audience to scan the code for the post, code and further resources. Leave this page visible while taking questions.

### Transition

> “I'm Shaun Lawrence. Thank you for choosing your own adventure.”

---

# Page-number map

Printed page numbers follow `PresentationDeck` story order (cover and optional page 404 excluded). Champion offsets apply from the chosen-champion summary onward. Inserting an opener automatically updates every destination and footer.

| Spread | Driver | Charioteer | Space Pilot |
| --- | ---: | ---: | ---: |
| ChosenChampion | 29 | 59 | 89 |
| GameLoop | 30 | 60 | 90 |
| InputChapter | 31 | 61 | 91 |
| MauiInput | 32 | 62 | 92 |
| InputPackage | 33 | 63 | 93 |
| VehicleControl | 34 | 64 | 94 |
| SignalRChapter | 35 | 65 | 95 |
| SignalRTransition | 36 | 66 | 96 |
| SignalRArchitecture | 37 | 67 | 97 |
| AudienceRace | 38 | 68 | 98 |
| EpilogueChapter | 39 | 69 | 99 |
| Podium | 40 | 70 | 100 |
| Recap | 41 | 71 | 101 |

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
- Right = firmware → champion walkthrough → Take Control

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
4. **SignalR walkthrough** — spend one sentence on each of the four stages and move to the race.
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



# Open-book presentation prototype

Each major beat is now a two-page spread. Headings and explanation occupy the left/right page headers; code samples, diagrams and the champion illustration sit on facing leaves. Voting instructions and QR code occupy the left page, with champion cards and destination numbers stacked on the right. The game-loop diagram and audience race can span both pages. Existing branch numbers refer to the spread, so “TURN TO PAGE 88” still matches its following footer.

Normal navigation plays a 680 ms paper-turn animation; Back reverses it. Wait for the leaf to settle before the next action. Tap anywhere on the left page to turn back on ordinary spreads; the right page continues. Tap anywhere on the cover to start. The voting lifecycle, stepping between loop stations, firmware reveal and local vehicle inputs stay within their current spread and do not animate a page turn.

Orbit runs one `BookScene`, which hosts the current scene and captures outgoing and incoming images during navigation so the turning leaf carries the next page’s content. The scene manager lifetime remains unchanged. `PresentationDeck` still defines the content order, and `IPresentationNavigator` owns next/previous/branch navigation.

### Controller navigation and confirmation

Use Left/Right shoulder for previous/next page or presentation beat (Page Up/Page Down on keyboard). South confirms the current action (Enter/Space); East cancels or resets it (Escape/Shift). The left stick and arrows/WASD select champions or control the vehicle. Navigation cannot open/close/reveal a vote or start a race: use confirmation for those commands. Forward navigation continues once a champion is revealed or a race has finished. In the vehicle demo South/East remain throttle/brake or thrust/down-thrust.

The live vehicle-control demonstration uses transparent motion sprites: rolling tyre treads for the driver, exhaust while the pilot applies thrust, and wheel movement with a dust trail for the chariot. Ground animation stops at rest; rocket exhaust stops when thrust is released. The paper-coloured oval backing has been removed.
