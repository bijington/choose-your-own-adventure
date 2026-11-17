# Repository layout

> Presenter rehearsal notes and the maintained walkthrough live in [`TALK-GUIDE.md`](TALK-GUIDE.md).

```text
src/
├── ChooseYourOwnAdventure.Presenter/   # .NET MAUI + Orbit presenter/game
└── Adventure.Web/                      # ASP.NET Core + SignalR audience site
```

The presenter and audience-facing web app are intentionally separated so it is clear which process runs on the conference machine and which process is deployed to Azure.

# Choose Your Own Adventure — Orbit + SignalR milestone

This version adds the first audience-participation milestone to the existing .NET MAUI + Orbit talk demo.

## What is new

- Existing controller / keyboard / touch presentation flow is preserved.
- `Adventure.Web` is a .NET 10 ASP.NET Core host with a SignalR hub.
- Audience members visit `/join/{sessionId}` and vote for Driver, Charioteer, or Space Pilot.
- The browser receives live totals via SignalR.
- The MAUI presenter app connects to the same hub and renders those totals in the Orbit champion-selection scene.
- A QR code is generated from the configured join URL and drawn directly with MAUI Graphics.
- Pressing A / Enter while votes exist chooses the current audience leader. If the web service is unavailable or there are no votes, the original local highlight/selection remains the fallback.

## Projects

The repository root is the existing MAUI presenter app:

- `ChooseYourOwnAdventure.csproj`

The audience web app is:

- `Adventure.Web/Adventure.Web.csproj`

## Run locally on Mac Catalyst

Start the web host first:

```bash
cd src/Adventure.Web
dotnet run --launch-profile http
```

It listens at:

```text
http://localhost:5187
```

Open this in a browser to simulate an audience member:

```text
http://localhost:5187/join/ORBIT
```

Then run the MAUI app from the repository root:

```bash
cd src/ChooseYourOwnAdventure.Presenter
dotnet build -t:Run -f net10.0-maccatalyst
```

The MAUI app intentionally starts even if SignalR is unavailable and retries the connection without blocking Orbit rendering.

> The default QR code points at `localhost`, which is useful only for local development on the presenter machine. A phone cannot reach your Mac through `localhost`. Set the deployed Azure URL before testing the QR code from another device.

## Configure the audience URL

Edit `Live/LiveAudienceOptions.cs`:

```csharp
public string BaseUrl { get; set; } = "https://YOUR-APP.azurewebsites.net";
public string SessionId { get; set; } = "ORBIT";
```

The presenter will then generate a QR code for:

```text
https://YOUR-APP.azurewebsites.net/join/ORBIT
```

## Azure hosting

The web project works in two modes:

1. **Local / simple App Service:** if no Azure SignalR connection string is configured, it uses normal ASP.NET Core SignalR directly.
2. **Azure SignalR Service:** when `Azure:SignalR:ConnectionString` is present, `Program.cs` calls `AddAzureSignalR()` automatically.

For App Service, add this application setting when you are ready to use Azure SignalR Service:

```text
Azure__SignalR__ConnectionString=<your Azure SignalR Service connection string>
```

Do not commit the connection string to source control.

The web project references `Microsoft.Azure.SignalR` 1.33.1. The MAUI app references `Microsoft.AspNetCore.SignalR.Client` 10.0.14.

## Voting behavior

Each browser stores a random participant ID locally. Voting again changes that participant's vote rather than adding another vote, and reconnecting does not intentionally create another participant.

The current vote state is intentionally in memory. Restarting the web app resets the votes. That is desirable for this first talk milestone and keeps the demo architecture easy to explain.

## Suggested next increment

Add presenter-only controls for:

- open voting
- close voting
- reset voting
- reveal winner

That gives the talk a deliberate dramatic beat before we reuse the same SignalR session for multiplayer race controls.

## Live champion voting lifecycle

The presenter now controls the audience vote from the `CHOOSE YOUR CHAMPION` screen:

- **A / Enter** while ready: open voting.
- **A / Enter** while voting is open: close voting and lock the result.
- **A / Enter** while closed: reveal the winner.
- **A / Enter** after the reveal: accept the winning champion and continue the adventure.
- **B / Shift**: reset the voting round for rehearsal or recovery.

If the result is tied, no hidden tie-break rule is applied. The presenter screen and audience devices show the tie, and **A / Enter** reopens voting for another round.

The audience page follows the same lifecycle: vote buttons are disabled until voting opens, locked when voting closes, and the winning champion is highlighted when the presenter reveals the result.

### Presenter control key

The vote lifecycle commands are protected by a presenter key so an audience browser cannot close or reveal the vote from developer tools.

Local development defaults to `orbit-local-presenter` on both apps. For Azure, set the web app setting/environment variable:

```text
Presenter__Key=<your-secret-presenter-key>
```

and set the same value in `LiveAudienceOptions.PresenterKey` for the presenter build. Do not expose the presenter key in the audience web page.

## Voting show moment

The presenter vote flow now includes presentation-side polish without adding extra server states:

- live vote bars ease toward the latest SignalR totals while voting is open;
- closing voting starts a three-second **LOCKING IN** countdown;
- the reveal begins with an **AND THE AUDIENCE CHOSE…** suspense beat;
- losing cards fade away before the winning champion gets the pulsing gold treatment;
- the reveal remains presenter-controlled, and repeated reveal presses are ignored until the lock-in countdown completes.

The server still only knows `Ready`, `Open`, `Closed`, and `Revealed`, so the theatrical timing stays local to the presenter and cannot add load or timing dependencies to the audience site.

## Apple application lifecycle

The Mac Catalyst head now uses `SceneDelegate : MauiUISceneDelegate` and an explicit multiple-scene manifest to support separate audience and presenter-notes windows. The manifest is included as a `PartialAppManifest` because this project intentionally omits `UseMaui`. The notes startup also accepts the SDK's `__MACCATALYST__` symbol. The iOS head remains unchanged.

Preserve the explicit Mac Catalyst RID/application-title workaround. The presentation starts before controller/network discovery and before notes are opened. This follows [MAUI's documented multi-window configuration](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/window?view=net-maui-10.0#multi-window-support).

## Theme-specific vehicle input

The vehicle-control scene now demonstrates different update models per champion. Driver and Charioteer keep the simple accelerate/brake course model. Space Pilot uses vertical thrust/gravity: hold Primary to apply upward thrust, release it to let gravity change velocity, and use Secondary/Down for downward thrust. The live code panel emphasizes the exact branch that is active.

## Theme-specific control demo

The vehicle-control scene now teaches a different game-programming concept for each audience-selected champion:

- **Racing Driver** — hold Primary for throttle and use Left/Right to steer across the circuit. The live code panel highlights velocity and steering state independently.
- **Charioteer** — Primary builds momentum, but releasing it applies only light drag, so the chariot keeps moving. Secondary brakes. This makes inertia/state persistence visible.
- **Space Pilot** — Primary applies upward thrust; releasing it lets gravity change velocity every update. Secondary adds downward thrust.

All three mechanics still pass through the same `AdventureInputRouter → Update() → Render()` pipeline.


## Audience champion race

The opening vote now also assigns every browser to a race team. The participant id is stored in browser local storage and the server keeps the authoritative `participant -> champion` mapping from the original vote.

After the individual vehicle-control demo, use **W / stick up** (or tap the bottom-right `AUDIENCE RACE` control) to open the race scene. Then:

- **A / Enter** starts the audience race.
- The existing audience voting page automatically switches to a large **BOOST** button.
- Every tap boosts the champion that participant originally voted for.
- **B / Escape** resets the race for rehearsal.
- After a finish, **A / Enter** starts another race.

All three champions are rendered together in Orbit. Race progress is server-authoritative and broadcast with SignalR. Browser taps are batched every 100ms so frantic audience tapping does not create one network call per physical tap.

To keep the race competitive when the opening vote creates uneven teams, each tap is normalised by that champion's team size. In practical terms, each team needs roughly the same *average taps per person* to finish, rather than the largest voting bloc automatically winning.

The race is intentionally tied to the opening vote: changing the DOM or button label in the browser cannot switch teams because the server looks up the participant's stored champion before applying a boost.


## Local SignalR browser client

The audience web app serves the SignalR JavaScript client locally from `wwwroot/js/signalr/dist/browser/signalr.min.js`; the audience page no longer loads SignalR from a CDN at runtime.

`src/Adventure.Web/libman.json` pins `@microsoft/signalr` to `10.0.11`, and `Microsoft.Web.LibraryManager.Build` restores that file during the web project build. This means an internet connection is only needed when restoring/building dependencies, not while the deployed conference site is running.

If you deliberately update the SignalR JavaScript client later, update the version in `libman.json` and rebuild the web project.

### Local SignalR browser client

`Adventure.Web/libman.json` restores `dist/browser/signalr.min.js` beneath `wwwroot/js/signalr/`, so the browser path is `/js/signalr/dist/browser/signalr.min.js`. The web app serves this file locally; audience devices do not depend on a CDN at runtime.

## Code font

The presenter app is configured to use Cascadia Code for rendered code samples.
Copy `CascadiaCode-VariableFont_wght.ttf` into:

`src/ChooseYourOwnAdventure.Presenter/Resources/Fonts/CascadiaCode-VariableFont_wght.ttf`

It is registered under the `CascadiaCode` alias in `MauiProgram.cs`. The `CodeFonts` helper provides regular, semibold and bold weights for the MAUI Graphics code panels.

## Presenter slide architecture

The presenter now uses one `GameScene` per major presentation beat. `PresentationDeck` declares the slide order and registers scenes; `IPresentationNavigator` loads them. `MainPage` handles slide-specific controls.

Current order:

1. `TitleScene`
2. `FirstDecisionScene`
3. `AfterPartyScene` — optional page-404 joke
4. `PrologueChapterScene`
5. `OrbitOverviewScene`
6. `InstallOrbitScene`
7. `GameSceneViewIntroScene`
8. `LoadGameSceneIntroScene`
9. `ChampionChapterScene`
10. `OpeningScene`
11. `ChosenChampionScene`
12. `InputChapterScene`
13. `InputPackageScene`
14. `GameLoopScene`
15. `VehicleControlScene`
16. `SignalRChapterScene`
17. `SignalRTransitionScene`
18. `SignalRArchitectureScene`
19. `AudienceRaceScene`
20. `EpilogueChapterScene`
21. `PodiumScene`
22. `RecapScene`

Chapter One opens before voting. The chosen-champion summary reveals its rendering code on the facing page, then continues to Take Control. The normal route skips the optional page-404 joke.

### Presenter controls

- `A` / Enter: advance ordinary slides.
- `B` / Escape: go back on ordinary slides (or reset where a live interaction owns Back).
- Champion vote: retains the existing Open → Close → Reveal → Continue lifecycle.
- Game loop: A advances around loop nodes; Right/D reveals firmware then proceeds to driving.
- Vehicle control: Up/W moves to the SignalR transition; normal controls continue to drive the vehicle.
- Audience race: A starts; after the race has finished A advances to the podium, then recap; B/Escape resets the race.

## Central presentation registration

The presenter slide deck is now declared in one place:

`src/ChooseYourOwnAdventure.Presenter/Presentation/PresentationDeck.cs`

`AddChooseYourOwnAdventurePresentation()` registers all slide scenes and the presentation navigator with dependency injection. The ordered slide definitions are also the source of truth for normal next/previous navigation.

`MainPage` resolves `IPresentationNavigator` and only handles interactions that are special to a slide (voting, game-loop stepping, vehicle controls, and the audience race). It no longer contains the slide order or concrete scene-loading switch.

## Title artwork

The presenter title scene uses `Resources/Images/choose_your_own_adventure_title.png`, embedded into the presenter assembly as `ChooseYourOwnAdventure.TitleScreen.png`. `TitleScene` renders it aspect-fit and overlays the animated start prompt.

## Presentation visual theme

The presenter uses a shared `PresentationTheme` (`src/ChooseYourOwnAdventure.Presenter/Presentation/PresentationTheme.cs`).

The deck now uses a warm parchment page as the common presentation canvas, with dark-brown typography, red/orange section accents, gold details, and light parchment inset panels for code. The title artwork remains its own full-screen illustrated cover, while the game-loop, control and audience-race scenes retain richer champion-specific visuals inside the parchment presentation frame.

When adding a new explanatory slide, prefer deriving from `PresentationSceneBase` and use `DrawShell`, `DrawTitle`, `DrawCodeCard`, `DrawPill` and `DrawPrompt` rather than hard-coding a new palette.

## Code sample theme

Code samples now use the same parchment visual language as the rest of the deck. Cascadia Code remains the code font, while code cards use a warm inset-paper background, brown ink, red-orange active lines, and gold/brown metrics instead of the previous dark IDE-style panels.

## Branch page numbers

Printed pages follow the story order in `PresentationDeck`, excluding the cover and optional page-404 joke. `PresentationTheme.GetPageNumber` adds the selected champion offset (Driver 20, Charioteer 50, Space Pilot 80) from the chosen-champion summary onward. Enum identities no longer dictate printed page order. The winning card points to summary page 29, 59 or 89; The rendering code appears within that same spread; Take Control follows on the next page. Presenter and web champion enum values and the browser numeric mapping still stay synchronized.

Vehicle artwork lives in `ChampionVehicleGameObject`, shared by `LoopVehicle`, `AudienceRaceVehicle` and podium `StaticChampionVehicleGameObject`s.

## Reliability checks

Run `dotnet run --project tests/Adventure.Rehearsal` for vote/race state checks and a 600-participant concurrency simulation. With the local web host running, append `-- http://localhost:5187` for live SignalR checks with three independent identities. See [rehearsal instructions](tests/Adventure.Rehearsal/README.md).

Before a conference, still rehearse keyboard/controller input, all three branches, real phones, projector readability, mobile reconnects and the deployed Azure host. Local concurrency checks do not measure Azure capacity or venue networking.

## Open-book prototype

The presenter now renders facing paper pages with a spine, layered edges and a short page-turn animation between major scenes. Explanations and code use the two leaves; the loop and race can span the book. The title stays as the cover, and existing branch page numbers identify spreads.

`BookScene` is the single scene loaded by Orbit's scene manager. It forwards update/render to the current presentation scene. Navigation captures the outgoing and incoming spreads into temporary bitmaps, then folds the outgoing right page across the spine over 680 ms. The reverse of the leaf shows the incoming left-page content; backwards navigation reverses the pages. The animation clock starts after capture finishes, and there is only one live update loop. Temporary images and content scopes are disposed when replaced. Input cannot skip through several spreads during a turn; local interactive beats do not turn a page.

On ordinary spreads, tap anywhere on the left page to turn back, or the right page to continue. Tap anywhere on the cover to start; existing keyboard/controller and special-scene controls still apply. Controller and SignalR discovery remain asynchronous.

Champion selection uses the left page for voting instructions, QR code and live status. Three stacked cards on the right show the Driver, Charioteer and Space Pilot, with their derived destination pages (29, 59 and 89). Touch selection uses the same card bounds as rendering; voting lifecycle controls are unchanged.

The champion vote reveals its winner in place: losing cards fade away and the winning card grows to fill the right page, retaining final vote counts and its destination. Continue opens a simple “OUR CHAMPION” summary with the selected name and real `ChampionGameObject`; the next action reveals rendering code on the left while the champion stays on the right. A further action opens Take Control. “MEET OUR CHAMPION” introduces Chapter One before voting, with the self-discovery lead-in and no selected character. On the vote spread, left-page taps and Escape go back; B/Shift resets voting. Offline selection uses the same reveal before continuing.

## Opening decision and chapter structure

After the cover, invite the room to shout a preference: **attend the talk → page 2**, or **go straight to the after-party → page 404**. This is presenter-controlled; no voting round opens. Tap the corresponding right-page choice, use A/Enter for the talk, or B/Shift for the shortcut. Page 404 says “The after-party could not be found. Please complete this adventure first.” A/Enter or a right-page tap then begins the prologue.

`PresentationChapter` supplies running labels on existing spreads, with a matching right-page opener for every section:

- Prologue · Discovering Orbit — overview, install, view and scene loading.
- Chapter One · Meet Your Champion — vote, GameObject and character reveal.
- Chapter Two · Take Control — input, loop and vehicle demo.
- Chapter Three · An Adventure Together — SignalR and audience race.
- Epilogue · The Journey Home — podium and recap.

The opening is page 1 and the joke is page 404. The prologue opens on page 2. Story pages are recalculated from deck order, while page 404 remains the optional joke.

The vote scene renders only voting content, independent of `OpeningStage`. Accepting a champion cannot switch its outgoing capture into the old character-reveal layout. The chapter scene reuses `ChampionGameObject` artwork without the dark panel or duplicate labels; its left leaf remains blank.

Every section now has a dedicated right-page chapter opener: Prologue, Meet Our Champion, Take Control, An Adventure Together and Epilogue. Each has a short narrative lead-in and a blank left leaf. The champion opener adds: “A little self-discovery. A few lines of C#. Let’s find out what our champion is made of.” The selected name and shared GameObject appear on the separate post-vote summary.

## Champion summary and rendering reveal

The champion appears once in the post-vote summary. A/Enter or a right-page tap fades in the real `Render()` excerpt and a champion-specific drawing-primitives example on the left; the right-page character remains unchanged. The next action opens Take Control. Left-page tap / Escape first hides the code, then returns to voting. The redundant Add GameObject and second character-reveal scenes have been removed; scene loading remains covered in the prologue.

## Private presenter-notes window

On Mac Catalyst and Windows, a separate notes window opens after the audience presentation starts. Put **Choose Your Own Adventure — Audience** on the projector and keep **Presenter Notes — Choose Your Own Adventure** on the laptop. Use an extended desktop rather than display mirroring. Only the audience window hosts Orbit.

Notes show the current spread/page, interactive beat, talking points, current controls, next action/spread, elapsed session time, audience connection, vote count and race phase. Controls and next action remain visible while longer notes scroll. Notes follow local beats such as rendering-code reveal, loop stations and firmware comparison; voting/tie and race-finish prompts reflect live state.

- **Presenter → Show Presenter Notes** opens or focuses the notes window without creating another presentation.
- **Return to presentation** brings the audience window forward without advancing it.
- Closing notes leaves the audience presentation running; reopening preserves the current state and session clock.
- Closing the audience window closes its notes window.

`TALK-GUIDE.md` is embedded at build time and supplies speaker prose. `PresenterCueFactory` adds concise cues for interactive beats. A notes-window dispatcher timer reads the existing shared state; there is no second Orbit loop or SignalR connection. Editing the guide requires rebuilding the presenter.

Mac Catalyst startup, automatic opening, live spread synchronization, return focus and close/reopen have been checked locally. Rehearse actual projector placement and full-screen behavior on the conference setup; Windows has not been exercised here.
