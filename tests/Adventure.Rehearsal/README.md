# Local rehearsal checks

Run the state/concurrency checks:

```sh
dotnet run --project tests/Adventure.Rehearsal
```

For live SignalR checks, start `Adventure.Web` on its HTTP launch profile, then run:

```sh
dotnet run --project tests/Adventure.Rehearsal -- http://localhost:5187
```

The live checks create a unique session, use three independent SignalR connections and participant IDs, and reset that session afterwards. They verify authorization, ties, reopen/reveal, locked teams, reconnects, race completion and locally served browser assets. The concurrency check exercises the store with 600 participants; it is not a 600-connection network load test.

For a manual presenter rehearsal, seed four votes in the **local ORBIT session** (this resets that session):

```sh
dotnet run --project tests/Adventure.Rehearsal -- --demo SpacePilot
```

`RacingDriver` and `Charioteer` are also accepted. Close/reveal/accept using the presenter. After starting a race seeded with SpacePilot, complete it with:

```sh
dotnet run --project tests/Adventure.Rehearsal -- --demo boost
```

These demo commands intentionally use only localhost and the local presenter key. Never use them against a live conference session.

The headless checks also verify presenter-note cues for the rendering reveal, firmware comparison, voting ties and race finish, plus extraction of speaker prose from the embedded talk guide. These checks do not open native windows; multi-window startup and screen placement require desktop rehearsal.

## Packaged presenter XAML

After building the Mac presenter, check the actual bundled assembly (not just the intermediate DLL):

```bash
dotnet run --project tests/Adventure.Rehearsal -- --check-presenter-xaml src/ChooseYourOwnAdventure.Presenter/bin/Debug/net10.0-maccatalyst/maccatalyst-arm64/ChooseYourOwnAdventure.app/Contents/MonoBundle/ChooseYourOwnAdventure.dll
```

This detects missing runtime XAML resources even when the build succeeds. The presenter explicitly keeps its XAML resources so Debug initialization and Hot Reload can load them after XamlC processing.

## Boost-style branch prototype

On `codex/boost-style-prototype`, **Choose Our Route** follows the vehicle-control demonstration and precedes Chapter Three. The audience votes for one route shared by all teams. South/Enter opens voting, closes and reveals the choice, then continues. East reopens the vote before racing. Left shoulder/Page Up goes back. In Practice mode, Up/Down (or the right-page cards) chooses a local preview; confirm to reveal, then confirm again to continue. A new audience session clears the choice.

- **The winding road (steady effort):** each accepted tap advances progress immediately.
- **Ancient launch pads (charge & release):** taps fill a shared team meter. Every quarter lap of earned progress releases automatically; excess charge carries forward. Each champion bursts four times to finish. The presenter and phone both show launch energy and brief JUMP feedback. The race has road markings or glowing launch-pad markers for the chosen route.
- Both modes keep 84 average taps per team member (1,680 for a 20-person team), roughly 28 seconds at three taps/second/person. The first team to finish ends the race. Ties, including no votes, take the winding road as stated on screen. Original champion votes still determine race teams.

**This prototype needs its matching backend and audience web client.** It adds `OpenBoostVoting`, `VoteBoostMode` and `CloseBoostVoting`, plus race-snapshot fields. Nothing has been deployed. Run it locally or on a separate test deployment; the existing deployed backend cannot run the new vote.

To run an isolated local test host:

```bash
dotnet run --no-launch-profile --project src/Adventure.Web -- --urls http://localhost:5199 --Azure:SignalR:ConnectionString=
dotnet run --project tests/Adventure.Rehearsal -- http://localhost:5199
```

Point `LiveAudience:BaseUrl` in your local presenter settings at that host to rehearse live. Physical phones need a reachable LAN/deployed URL, not `localhost`. The localhost checks cover both normal and burst finishes, registration/phase guards, presenter authorization, reconnect and matching 20-person team effort.
