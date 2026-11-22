using Adventure.Web.Models;
using Adventure.Web.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

// Optional manual desktop rehearsal, using only the local ORBIT session.
if (args.Length >= 2 && args[0] == "--demo")
{
    await using var demo = new HubConnectionBuilder().WithUrl("http://localhost:5187/adventureHub").Build();
    await demo.StartAsync();
    if (args[1] == "boost")
    {
        foreach (var participant in new[] { "demo-SpacePilot", "demo-extra" })
        foreach (var taps in new[] { 20, 20, 20, 20, 4 })
            await demo.InvokeAsync("Boost", "ORBIT", participant, taps);
    }
    else
    {
        Check(Enum.TryParse<Champion>(args[1], out var selected) && Enum.IsDefined(selected), "Specify a champion or boost");
        await demo.InvokeAsync("ResetVoting", "ORBIT", "orbit-local-presenter");
        await demo.InvokeAsync("OpenVoting", "ORBIT", "orbit-local-presenter");
        foreach (var name in Enum.GetNames<Champion>())
            await demo.InvokeAsync("Vote", "ORBIT", "demo-" + name, name);
        await demo.InvokeAsync("Vote", "ORBIT", "demo-extra", selected.ToString());
    }
    return;
}

foreach (var champion in Enum.GetValues<Champion>())
    Check((int)champion == (int)Enum.Parse<ChooseYourOwnAdventure.Champion>(champion.ToString()), "Champion contract mismatch");
Check(Enum.GetValues<ChooseYourOwnAdventure.PresentationSlide>().Select(x => (int)x).SequenceEqual(Enumerable.Range(1, 25).Where(id => id is not (8 or 9)).Append(404)), "Slide numbering mismatch");

// Local development overrides must reach both the SignalR client and the QR code.
using (var defaultSettings = typeof(ChooseYourOwnAdventure.Live.LiveAudienceOptions).Assembly
    .GetManifestResourceStream("Rehearsal.PresenterSettings.json")!)
using (var developmentSettings = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("""
{"LiveAudience":{"BaseUrl":"https://audience.example.test/","PresenterKey":"test-only-key"}}
""")))
{
    using var configuration = new ConfigurationManager();
    configuration.AddJsonStream(defaultSettings).AddJsonStream(developmentSettings);
    var options = ChooseYourOwnAdventure.Live.LiveAudienceOptions.FromConfiguration(configuration);
    Check(options.BaseUrl == "https://audience.example.test/" && options.PresenterKey == "test-only-key",
        "Development connection settings did not override defaults");
    Check(options.SessionId == "ORBIT", "Partial development override lost the default session");
    Check(options.HubUrl == "https://audience.example.test/adventureHub"
        && options.JoinUrl == "https://audience.example.test/join/ORBIT", "Hub and QR URLs did not use configuration");
}
Console.WriteLine("PASS: presenter JSON overrides configure the hub, QR URL and presenter key.");

// An unconfigured or invalid resources link must never become a misleading QR code.
using (var summaryConfig = new ConfigurationManager())
{
    var summary = ChooseYourOwnAdventure.Presentation.TalkSummaryOptions.FromConfiguration(summaryConfig);
    Check(summary.SpeakerName == "Shaun Lawrence" && summary.ResourcesUri is null,
        "Unconfigured summary did not use the speaker name and empty-link state");
    foreach (var invalid in new[] { "", "not a URL", "javascript:alert(1)", "ftp://example.test/resources" })
    {
        summary.ResourcesUrl = invalid;
        Check(summary.ResourcesUri is null, "Invalid resources link would create a QR code");
    }
    summaryConfig["TalkSummary:ResourcesUrl"] = "https://example.test/talk-resources";
    summary = ChooseYourOwnAdventure.Presentation.TalkSummaryOptions.FromConfiguration(summaryConfig);
    Check(summary.ResourcesUri?.AbsoluteUri == "https://example.test/talk-resources",
        "Configured blog URL did not reach the closing summary");
}
Console.WriteLine("PASS: summary configuration, speaker name, valid resources URL and empty/invalid-link fallback.");

// Practice must not even construct a hub connection or send presenter commands.
var practiceState = new ChooseYourOwnAdventure.AdventureState { IsRehearsing = true };
var practiceVote = new ChooseYourOwnAdventure.Live.LiveVoteState();
var practiceRace = new ChooseYourOwnAdventure.Live.LiveRaceState();
await using (var practiceClient = new ChooseYourOwnAdventure.Live.AudienceVoteClient(
    new ChooseYourOwnAdventure.Live.LiveAudienceOptions { BaseUrl = "not a usable connection URL" },
    practiceVote, practiceRace, practiceState))
{
    await practiceClient.StartAsync();
    await practiceClient.OpenVotingAsync();
    await practiceClient.CloseVotingAsync();
    await practiceClient.RevealWinnerAsync();
    await practiceClient.ResetVotingAsync();
    await practiceClient.StartRaceAsync();
    await practiceClient.ResetRaceAsync();
    Check(!practiceClient.IsConnected && practiceVote.Snapshot.Phase == ChooseYourOwnAdventure.Live.VotePhase.Ready
        && practiceRace.Snapshot.Phase == ChooseYourOwnAdventure.Live.RacePhase.Ready,
        "Practice connected to or changed the live session");
}
Check(ChooseYourOwnAdventure.Input.PresentationNavigation.ResolveNext(
    ChooseYourOwnAdventure.PresentationSlide.AudienceRace, practiceState, practiceVote.Snapshot, practiceRace.Snapshot, false)
    == ChooseYourOwnAdventure.Input.AdventureAction.Confirm, "Practice could not skip an unstarted race");
Console.WriteLine("PASS: practice skips the race and suppresses connection setup and presenter commands.");

// Navigation must not accidentally issue confirmation commands to live interactions.
var navState = new ChooseYourOwnAdventure.AdventureState();
var navVote = new ChooseYourOwnAdventure.Live.VoteSnapshot("ORBIT", 0, 0, 0,
    ChooseYourOwnAdventure.Live.VotePhase.Ready, null);
var navRace = new ChooseYourOwnAdventure.Live.RaceSnapshot("ORBIT", 0, 0, 0, 0, 0, 0,
    ChooseYourOwnAdventure.Live.RacePhase.Ready, null);
foreach (var phase in Enum.GetValues<ChooseYourOwnAdventure.Live.VotePhase>())
    Check(ChooseYourOwnAdventure.Input.PresentationNavigation.ResolveNext(
        ChooseYourOwnAdventure.PresentationSlide.ChooseChampion, navState, navVote with { Phase = phase }, navRace, true) is null,
        "Navigation started voting, revealed a result or accepted a tie");
Check(ChooseYourOwnAdventure.Input.PresentationNavigation.ResolveNext(
    ChooseYourOwnAdventure.PresentationSlide.ChooseChampion, navState,
    navVote with { Phase = ChooseYourOwnAdventure.Live.VotePhase.Revealed, RevealedChampion = ChooseYourOwnAdventure.Champion.SpacePilot },
    navRace, true) == ChooseYourOwnAdventure.Input.AdventureAction.Confirm, "Navigation could not accept a revealed winner");
foreach (var phase in Enum.GetValues<ChooseYourOwnAdventure.Live.RacePhase>())
    Check(ChooseYourOwnAdventure.Input.PresentationNavigation.ResolveNext(
        ChooseYourOwnAdventure.PresentationSlide.AudienceRace, navState, navVote, navRace with { Phase = phase }, true)
        == (phase == ChooseYourOwnAdventure.Live.RacePhase.Finished ? ChooseYourOwnAdventure.Input.AdventureAction.Confirm : null),
        "Race navigation started or interrupted a race");
Console.WriteLine("PASS: navigation waits for a revealed champion or finished race without starting live interactions.");

// The architecture spread must finish all four steps before forward input can leave it.
var walkthroughState = new ChooseYourOwnAdventure.AdventureState();
Check(!walkthroughState.PreviousSignalRStage(), "Walkthrough underflowed its first step");
foreach (var stage in Enum.GetValues<ChooseYourOwnAdventure.SignalRStage>())
{
    Check(walkthroughState.SignalRStage == stage, "Walkthrough skipped a component");
    Check(ChooseYourOwnAdventure.Input.PresentationNavigation.ResolveNext(
        ChooseYourOwnAdventure.PresentationSlide.SignalRArchitecture, walkthroughState, navVote, navRace, false)
        == ChooseYourOwnAdventure.Input.AdventureAction.Confirm, "Shoulder navigation bypassed the walkthrough");
    var stepCue = ChooseYourOwnAdventure.PresenterNotes.PresenterCueFactory.Create(
        ChooseYourOwnAdventure.PresentationSlide.SignalRArchitecture, walkthroughState, navVote, navRace, false);
    Check(stepCue.Beat.Contains($"{(int)stage + 1}/4") && stepCue.TalkingPoints.Length > 0,
        "Presenter notes did not follow the SignalR stage");
    Check(walkthroughState.AdvanceSignalRStage() == (stage != ChooseYourOwnAdventure.SignalRStage.Presenter),
        "Walkthrough left early or advanced beyond the last step");
}
Check(walkthroughState.SignalRStage == ChooseYourOwnAdventure.SignalRStage.Presenter,
    "Walkthrough changed after its final step");
for (var i = 0; i < 3; i++) Check(walkthroughState.PreviousSignalRStage(), "Could not reverse through the walkthrough");
walkthroughState.AdvanceSignalRStage();
walkthroughState.ResetSignalRWalkthrough();
Check(walkthroughState.SignalRStage == ChooseYourOwnAdventure.SignalRStage.Browser, "Re-entering did not reset the walkthrough");
Console.WriteLine("PASS: four-stage SignalR walkthrough, forward/back bounds, shoulder navigation and presenter cues.");

// The overview teaches all four building blocks before moving to installation.
var overviewState = new ChooseYourOwnAdventure.AdventureState();
Check(!overviewState.PreviousOrbitOverviewStage(), "Orbit overview underflowed its first step");
foreach (var stage in Enum.GetValues<ChooseYourOwnAdventure.OrbitOverviewStage>())
{
    Check(overviewState.OrbitOverviewStage == stage, "Orbit overview skipped a building block");
    Check(ChooseYourOwnAdventure.Input.PresentationNavigation.ResolveNext(
        ChooseYourOwnAdventure.PresentationSlide.OrbitOverview, overviewState, navVote, navRace, false)
        == ChooseYourOwnAdventure.Input.AdventureAction.Confirm, "Shoulder navigation skipped the overview");
    var cue = ChooseYourOwnAdventure.PresenterNotes.PresenterCueFactory.Create(
        ChooseYourOwnAdventure.PresentationSlide.OrbitOverview, overviewState, navVote, navRace, false);
    Check(cue.Beat.Contains($"{(int)stage + 1}/4") && cue.TalkingPoints.Length > 0,
        "Overview notes did not track the building block");
    Check(overviewState.AdvanceOrbitOverviewStage() == (stage != ChooseYourOwnAdventure.OrbitOverviewStage.Game),
        "Orbit overview left early or overflowed its last step");
}
for (var i = 0; i < 3; i++) Check(overviewState.PreviousOrbitOverviewStage(), "Could not reverse the overview");
overviewState.AdvanceOrbitOverviewStage();
overviewState.ResetOrbitOverview();
Check(overviewState.OrbitOverviewStage == ChooseYourOwnAdventure.OrbitOverviewStage.Maui,
    "Re-entering did not reset the Orbit overview");
Console.WriteLine("PASS: four-stage Orbit overview, navigation bounds and stage-specific presenter notes.");

// Presenter notes must follow interactive beats, not just the scene index.
var presenterState = new ChooseYourOwnAdventure.AdventureState();
var readyVote = new ChooseYourOwnAdventure.Live.VoteSnapshot("ORBIT", 0, 0, 0,
    ChooseYourOwnAdventure.Live.VotePhase.Ready, null);
var readyRace = new ChooseYourOwnAdventure.Live.RaceSnapshot("ORBIT", 0, 0, 0, 0, 0, 0,
    ChooseYourOwnAdventure.Live.RacePhase.Ready, null);
ChooseYourOwnAdventure.PresenterNotes.PresenterCue Cue(ChooseYourOwnAdventure.PresentationSlide slide,
    ChooseYourOwnAdventure.Live.VoteSnapshot? vote = null,
    ChooseYourOwnAdventure.Live.RaceSnapshot? raceSnapshot = null) =>
    ChooseYourOwnAdventure.PresenterNotes.PresenterCueFactory.Create(slide, presenterState,
        vote ?? readyVote, raceSnapshot ?? readyRace, true);
Check(Cue(ChooseYourOwnAdventure.PresentationSlide.ChosenChampion).NextAction.Contains("four-stage"),
    "Summary notes skipped the rendering evolution");
Check(!presenterState.PreviousChampionRendering(), "Rendering evolution underflowed the summary");
foreach (var stage in Enum.GetValues<ChooseYourOwnAdventure.ChampionRenderingStage>())
{
    Check(presenterState.AdvanceChampionRendering(), "Rendering evolution left before the sprite stage");
    Check(presenterState.ChampionRenderingStage == stage, "Rendering evolution skipped a stage");
    Check(Cue(ChooseYourOwnAdventure.PresentationSlide.ChosenChampion).Beat.Contains($"{(int)stage+1}/4"),
        "Notes did not follow the rendering stage");
    Check(ChooseYourOwnAdventure.Input.PresentationNavigation.ResolveNext(
        ChooseYourOwnAdventure.PresentationSlide.ChosenChampion, presenterState, readyVote, readyRace, true)
        == ChooseYourOwnAdventure.Input.AdventureAction.Confirm, "Shoulder navigation skipped the evolution");
}
Check(!presenterState.AdvanceChampionRendering()
    && Cue(ChooseYourOwnAdventure.PresentationSlide.ChosenChampion).NextAction.Contains("frame in the life"),
    "Rendering evolution did not finish at the loop handoff");
for (var i=0; i<4; i++) Check(presenterState.PreviousChampionRendering(), "Could not reverse the rendering evolution");
Check(!presenterState.ChampionRenderingVisible, "Reversing past the dot did not restore the summary");
presenterState.ShowChampionRendering();
presenterState.PrepareChampionReveal();
Check(!presenterState.ChampionRenderingVisible, "Re-entering the summary retained a rendering stage");
presenterState.ShowFirmwareComparison();
Check(Cue(ChooseYourOwnAdventure.PresentationSlide.GameLoop).NextAction.Contains("frame in the life"),
    "Firmware should introduce the champion walking around the loop");
presenterState.ShowGameLoopOverview();
Check(Cue(ChooseYourOwnAdventure.PresentationSlide.GameLoop).NextAction.Contains("Take Control"),
    "Character loop should lead into Chapter Two");
presenterState.ResetGameLoop();
Check(presenterState.GameLoopStage == ChooseYourOwnAdventure.GameLoopStage.FirmwareComparison
    && presenterState.GameLoopStation == ChooseYourOwnAdventure.LoopStation.Input,
    "Entering the game-loop spread should start with firmware before the champion walkthrough");
var tiedVote = readyVote with { Phase = ChooseYourOwnAdventure.Live.VotePhase.Revealed,
    RacingDriver = 1, Charioteer = 1, SpacePilot = 1 };
Check(Cue(ChooseYourOwnAdventure.PresentationSlide.ChooseChampion, tiedVote).NextAction.Contains("Reopen voting"),
    "Tied-vote notes incorrectly continued to a champion");
Check(Cue(ChooseYourOwnAdventure.PresentationSlide.AudienceRace,
    raceSnapshot: readyRace with { Phase = ChooseYourOwnAdventure.Live.RacePhase.Finished,
    Winner = ChooseYourOwnAdventure.Champion.SpacePilot }).NextAction.Contains("The Journey Home"),
    "Finished-race notes skipped the epilogue");
var offlineCue = ChooseYourOwnAdventure.PresenterNotes.PresenterCueFactory.Create(
    ChooseYourOwnAdventure.PresentationSlide.ChooseChampion, new ChooseYourOwnAdventure.AdventureState(),
    tiedVote, readyRace, false);
Check(offlineCue.Beat == "Local champion selection" && offlineCue.Controls.Contains("Up/Down")
    && offlineCue.NextAction.Contains("highlighted champion"),
    "Offline notes showed stale live-vote state instead of vertical selection and local confirmation");
var localSelection = new ChooseYourOwnAdventure.AdventureState();
localSelection.HighlightNextChampion();
localSelection.RevealLocalVote();
Check(ChooseYourOwnAdventure.PresenterNotes.PresenterCueFactory.Create(
    ChooseYourOwnAdventure.PresentationSlide.ChooseChampion, localSelection, readyVote, readyRace, true)
    .Beat == "Local champion selection", "Reconnecting discarded the local winner's presentation cue");
Check(ChooseYourOwnAdventure.Presentation.PresentationChapter.LabelFor(ChooseYourOwnAdventure.PresentationSlide.GameLoop)
    == "CHAPTER ONE · MEET OUR CHAMPION", "Game loop is still labelled Chapter Two");
foreach (var champion in Enum.GetValues<ChooseYourOwnAdventure.Champion>())
{
    foreach (var station in Enum.GetValues<ChooseYourOwnAdventure.LoopStation>())
    {
        var beat = ChooseYourOwnAdventure.Presentation.ChampionLoopStory.For(champion, station);
        Check(beat.Thought.Length > 0 && beat.Explanation.Length > 0, "Champion loop story is missing a beat");
    }
}
presenterState.ShowGameLoopOverview();
Check(presenterState.GameLoopStage == ChooseYourOwnAdventure.GameLoopStage.Overview,
    "Could not return from firmware comparison to the champion loop");
Check(ChooseYourOwnAdventure.Presentation.PresentationChapter.LabelFor(ChooseYourOwnAdventure.PresentationSlide.MauiInput)
    == "CHAPTER TWO · TAKE CONTROL", "MAUI input spread lost its chapter label");
Check(Cue(ChooseYourOwnAdventure.PresentationSlide.MauiInput).TalkingPoints.Contains("Pressed and Released")
    && Cue(ChooseYourOwnAdventure.PresentationSlide.MauiInput).NextAction.Contains("Orbit.Input"),
    "MAUI input notes do not explain existing controls before the input package");
var parsedGuide = new ChooseYourOwnAdventure.PresenterNotes.PresenterNotesGuide(
    "## Sample — `TitleScene`\n### Talking points\n- **Keep this point.**\n```csharp\nsecret code\n## Bogus — `OpeningScene`\n```\n## Another section\nDo not include this.");
Check(parsedGuide.ForScene("TitleScene").Contains("Keep this point.")
    && !parsedGuide.ForScene("TitleScene").Contains("secret code")
    && !parsedGuide.ForScene("TitleScene").Contains("Do not include"),
    "Talk-guide parsing leaked code or another section");
using (var guideStream = typeof(ChooseYourOwnAdventure.PresenterNotes.PresenterNotesGuide).Assembly
    .GetManifestResourceStream("Rehearsal.TalkGuide.md")!)
using (var guideReader = new StreamReader(guideStream))
{
    var realGuide = new ChooseYourOwnAdventure.PresenterNotes.PresenterNotesGuide(guideReader.ReadToEnd());
    Check(realGuide.ForScene("ChosenChampionScene").Contains("Sprite", StringComparison.Ordinal),
        "Maintained guide no longer supplies champion-rendering notes");
}
Console.WriteLine("PASS: presenter notes track local rendering, firmware, vote ties and race finish; guide parser extracts prose and excludes code.");

var store = new VoteStore();
const string loadSession = "LOAD";
store.OpenVoting(loadSession);
await Task.WhenAll(Enumerable.Range(0, 600).Select(i => Task.Run(() =>
    store.CastVote(loadSession, $"p{i}", Enum.GetValues<Champion>()[i % 3]))));
Check(store.CloseVoting(loadSession).Total == 600, "Concurrent votes lost");
Check(store.RevealWinner(loadSession).RevealedChampion is null, "Tie picked a winner");
store.StartRace(loadSession);
await Task.WhenAll(Enumerable.Range(0, 600).Select(i => Task.Run(() =>
{
    foreach (var taps in new[] { 20, 20, 2 }) store.Boost(loadSession, $"p{i}", taps);
})));
var race = store.GetRaceSnapshot(loadSession);
Check(new[] {race.RacingDriverProgress, race.CharioteerProgress, race.SpacePilotProgress}.All(x => Math.Abs(x - .5) < .00001), "Team normalization incorrect");
Check(store.Boost(loadSession, "unknown", 20) == race, "Unregistered boost accepted");
Check(store.Boost(loadSession, "p0", 0) == race && store.Boost(loadSession, "p0", -5) == race, "Non-positive taps accepted");
store.CastVote(loadSession, "p0", Champion.SpacePilot);
Check(store.GetSnapshot(loadSession).RacingDriver == 200, "Closed vote changed team");
store.OpenVoting("BATCHED");
store.CastVote("BATCHED", "a", Champion.SpacePilot);
store.CastVote("BATCHED", "b", Champion.SpacePilot);
store.CloseVoting("BATCHED");
store.StartRace("BATCHED");
foreach (var participant in new[] { "a", "b" })
{
    for (var batch = 0; batch < 4; batch++) store.Boost("BATCHED", participant, 20);
    store.Boost("BATCHED", participant, 4);
}
Check(store.GetRaceSnapshot("BATCHED").Phase == RacePhase.Finished, "Batched roundoff prevented race finish");
Console.WriteLine("PASS: batched finish roundoff regression.");
Console.WriteLine("PASS: 600 concurrent participants, tie handling, team normalization, invalid taps, locked teams and enum contracts.");

if (args.Length == 0) return;
var url = args[0].TrimEnd('/');
var session = "REHEARSAL-" + Guid.NewGuid().ToString("N");
const string key = "orbit-local-presenter";
await using var presenter = new HubConnectionBuilder().WithUrl(url + "/adventureHub").Build();
await presenter.StartAsync();
await presenter.InvokeAsync("JoinSession", session);
var clients = new List<HubConnection>();
var votes = new List<VoteSnapshot>();
var races = new List<RaceSnapshot>();
try
{
    for (var i = 0; i < 3; i++)
    {
        var client = new HubConnectionBuilder().WithUrl(url + "/adventureHub").Build();
        client.On<VoteSnapshot>("VoteUpdated", x => { lock (votes) votes.Add(x); });
        client.On<RaceSnapshot>("RaceUpdated", x => { lock (races) races.Add(x); });
        clients.Add(client);
        await client.StartAsync();
        await client.InvokeAsync("JoinSession", session);
    }
    async Task Until(Func<bool> predicate, string message)
    {
        var deadline = DateTime.UtcNow.AddSeconds(8);
        while (!predicate() && DateTime.UtcNow < deadline) await Task.Delay(20);
        Check(predicate(), message);
    }
    bool HasVote(Func<VoteSnapshot, bool> f) { lock (votes) return votes.Any(f); }
    bool HasRace(Func<RaceSnapshot, bool> f) { lock (races) return races.Any(f); }
    try { await clients[0].InvokeAsync("OpenVoting", session, "wrong-key"); throw new Exception("Bad key accepted"); }
    catch (Microsoft.AspNetCore.SignalR.HubException) { }
    await presenter.InvokeAsync("OpenVoting", session, key);
    var champions = Enum.GetNames<Champion>();
    for (var i = 0; i < 3; i++) await clients[i].InvokeAsync("Vote", session, $"p{i}", champions[i]);
    try { await clients[0].InvokeAsync("Vote", session, "invalid", "999"); throw new Exception("Invalid champion accepted"); }
    catch (Microsoft.AspNetCore.SignalR.HubException) { }
    await presenter.InvokeAsync("CloseVoting", session, key);
    await presenter.InvokeAsync("RevealWinner", session, key);
    await Until(() => HasVote(x => x.Phase == VotePhase.Revealed && x.Total == 3 && x.RevealedChampion is null), "Live tie failed");
    await presenter.InvokeAsync("OpenVoting", session, key);
    await clients[0].InvokeAsync("Vote", session, "extra", "RacingDriver");
    await presenter.InvokeAsync("CloseVoting", session, key);
    await presenter.InvokeAsync("RevealWinner", session, key);
    await Until(() => HasVote(x => x.RevealedChampion == Champion.RacingDriver), "Live winner failed");
    await presenter.InvokeAsync("StartRace", session, key);
    await clients[2].InvokeAsync("Vote", session, "p2", "RacingDriver");
    foreach (var taps in new[] { 20, 20, 2 })
        await clients[2].InvokeAsync("Boost", session, "p2", taps);
    await Until(() => HasRace(x => Math.Abs(x.SpacePilotProgress - .5) < .00001 && x.RacingDriverProgress == 0), "Original race team lost");
    await clients[2].StopAsync();
    await clients[2].StartAsync();
    await clients[2].InvokeAsync("JoinSession", session);
    foreach (var taps in new[] { 20, 20, 2 })
        await clients[2].InvokeAsync("Boost", session, "p2", taps);
    await Until(() => HasRace(x => x.Phase == RacePhase.Finished && x.Winner == Champion.SpacePilot), "Reconnect/finish failed");
    using var http = new HttpClient();
    Check((await http.GetAsync(url + "/join/ORBIT")).IsSuccessStatusCode, "Join page failed");
    Check((await http.GetStringAsync(url + "/js/signalr/dist/browser/signalr.min.js")).Length > 10000, "Local SignalR client missing");
    Console.WriteLine("PASS: live SignalR with three identities, presenter authorization, invalid champion rejection, tie/reopen/reveal, original teams, reconnect, race finish, join URL and local browser client.");
}
finally
{
    await presenter.InvokeAsync("ResetVoting", session, key);
    foreach (var client in clients) await client.DisposeAsync();
}
