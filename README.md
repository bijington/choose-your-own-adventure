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
