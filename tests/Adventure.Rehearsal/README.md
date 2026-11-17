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
