# Repository layout

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
- **B / Escape**: reset the voting round for rehearsal or recovery.

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

## Apple UIScene lifecycle

The iOS and Mac Catalyst heads include an explicit `SceneDelegate` and `UIApplicationSceneManifest` for current Apple SDKs. After changing these files, clean generated outputs before rebuilding:

```bash
rm -rf src/ChooseYourOwnAdventure.Presenter/bin src/ChooseYourOwnAdventure.Presenter/obj
dotnet restore
dotnet build -t:Run -f net10.0-maccatalyst src/ChooseYourOwnAdventure.Presenter/ChooseYourOwnAdventure.csproj
```

## Latest presenter transition

The winning champion now transitions directly into the Orbit code reveal. After the audience winner is accepted, the champion is shown centre-stage. Press A/Enter once more and the same `ChampionGameObject` animates into the right-hand code panel while the real Orbit code panel fades in on the left.

This transition is presenter-side only; the SignalR vote lifecycle and web host are unchanged.
