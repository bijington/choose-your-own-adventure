using Adventure.Contracts;
using Adventure.Web.Models;
using Adventure.Web.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

// Inspect the packaged assembly without loading the platform-specific MAUI runtime.
if (args.Length == 2 && args[0] == "--check-presenter-xaml")
{
    using var stream = File.OpenRead(args[1]);
    using var pe = new System.Reflection.PortableExecutable.PEReader(stream);
    var metadata = System.Reflection.Metadata.PEReaderExtensions.GetMetadataReader(pe);
    var resources = metadata.ManifestResources.Select(handle =>
        metadata.GetString(metadata.GetManifestResource(handle).Name)).ToHashSet();
    foreach (var name in new[]
    {
        "ChooseYourOwnAdventure.App.xaml",
        "ChooseYourOwnAdventure.MainPage.xaml",
        "ChooseYourOwnAdventure.Controls.MauiAnimationShowcase.xaml",
        "ChooseYourOwnAdventure.Controls.ParticipationQrView.xaml",
        "ChooseYourOwnAdventure.PresenterNotes.PresenterNotesPage.xaml"
    })
        Check(resources.Contains(name), $"Packaged presenter is missing embedded XAML: {name}");
    Console.WriteLine("PASS: packaged presenter retains XAML for application, main page, animation/participation controls and presenter notes.");
    return;
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
Check(Enum.GetValues<ChooseYourOwnAdventure.PresentationSlide>().Select(x => (int)x).SequenceEqual(Enumerable.Range(1, 29).Where(id => id is not (8 or 9)).Append(404).Append(405)), "Slide numbering mismatch");

foreach (var mode in Enum.GetValues<BoostMode>())
    Check((int)mode == (int)Enum.Parse<ChooseYourOwnAdventure.Live.BoostMode>(mode.ToString()),
        "Boost mode contract mismatch");
var ruleNavState = new ChooseYourOwnAdventure.AdventureState();
var ruleVote = new ChooseYourOwnAdventure.Live.VoteSnapshot("RULES", 1, 0, 0, ChooseYourOwnAdventure.Live.VotePhase.Closed, null);
var ruleRace = new ChooseYourOwnAdventure.Live.RaceSnapshot("RULES", 0, 0, 0, 1, 0, 0, ChooseYourOwnAdventure.Live.RacePhase.Ready, null);
foreach (var phase in new[] { ChooseYourOwnAdventure.Live.VotePhase.Ready, ChooseYourOwnAdventure.Live.VotePhase.Open })
    Check(ChooseYourOwnAdventure.Input.PresentationNavigation.ResolveNext(ChooseYourOwnAdventure.PresentationSlide.BoostChoice,
        ruleNavState, ruleVote, ruleRace with { BoostVotePhase = phase }, true) is null,
        "Forward navigation started or skipped rule voting");
Check(ChooseYourOwnAdventure.Input.PresentationNavigation.ResolveNext(ChooseYourOwnAdventure.PresentationSlide.BoostChoice,
    ruleNavState, ruleVote, ruleRace with { BoostVotePhase = ChooseYourOwnAdventure.Live.VotePhase.Closed }, true)
    == ChooseYourOwnAdventure.Input.AdventureAction.Confirm, "Closed rule vote could not continue");

var participationCode = new ChooseYourOwnAdventure.Presentation.ParticipationQrState();
participationCode.Visit(ChooseYourOwnAdventure.PresentationSlide.Title);
Check(!participationCode.CanShow && !participationCode.IsExpanded, "Cover showed audience QR");
participationCode.Visit(ChooseYourOwnAdventure.PresentationSlide.ChooseChampion);
Check(participationCode.IsExpanded, "First champion entry did not show large QR");
Check(participationCode.Dismiss() && !participationCode.Dismiss(), "QR dismissal did not consume exactly one action");
participationCode.Visit(ChooseYourOwnAdventure.PresentationSlide.ChooseChampion);
Check(!participationCode.IsExpanded, "Polling reopened a dismissed QR");
foreach (var participationSlide in new[] { ChooseYourOwnAdventure.PresentationSlide.BoostChoice,
    ChooseYourOwnAdventure.PresentationSlide.AudienceRace, ChooseYourOwnAdventure.PresentationSlide.AfterCreditsRace })
{
    participationCode.Visit(participationSlide);
    Check(participationCode.CanShow && !participationCode.IsExpanded, "Later participation slides unexpectedly interrupted the flow");
    participationCode.Toggle();
    Check(participationCode.IsExpanded, "Manual QR enlargement failed");
}
participationCode.Visit(ChooseYourOwnAdventure.PresentationSlide.Recap);
Check(!participationCode.CanShow && !participationCode.IsExpanded, "Audience code leaked onto resources page");
participationCode.Visit(ChooseYourOwnAdventure.PresentationSlide.ChooseChampion);
Check(!participationCode.IsExpanded, "Revisiting the champion vote showed the initial QR again");
participationCode.ResetSession();
participationCode.Visit(ChooseYourOwnAdventure.PresentationSlide.ChooseChampion);
Check(participationCode.IsExpanded, "Fresh session did not show its new join QR");
Console.WriteLine("PASS: initial audience QR, single dismissal, manual enlargement, scene changes and session reset.");

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
var practiceOptions = new ChooseYourOwnAdventure.Live.LiveAudienceOptions { BaseUrl = "not a usable connection URL" };
await using (var practiceClient = new ChooseYourOwnAdventure.Live.AudienceVoteClient(
    practiceOptions,
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
    practiceVote.Update(new("ORBIT", 20, 10, 5, ChooseYourOwnAdventure.Live.VotePhase.Revealed,
        ChooseYourOwnAdventure.Champion.RacingDriver));
    practiceRace.Update(new("ORBIT", 1, .5, .2, 20, 10, 5, ChooseYourOwnAdventure.Live.RacePhase.Finished,
        ChooseYourOwnAdventure.Champion.RacingDriver));
    practiceState.RevealLocalVote();
    Check(practiceState.TryUnlockAfterCreditsAchievement() && !practiceState.TryUnlockAfterCreditsAchievement(),
        "After-credits achievement did not unlock exactly once");
    var freshSession = await practiceClient.CreateSessionAsync();
    Check(practiceState.TryUnlockAfterCreditsAchievement(), "New audience session did not reset the achievement");
    Check(freshSession != "ORBIT" && freshSession == practiceOptions.SessionId
        && practiceOptions.JoinUrl.EndsWith("/join/" + freshSession), "New session did not update the audience link");
    Check(practiceVote.Snapshot.SessionId == freshSession && practiceVote.Snapshot.Total == 0
        && practiceVote.Snapshot.Phase == ChooseYourOwnAdventure.Live.VotePhase.Ready
        && practiceRace.Snapshot.SessionId == freshSession
        && practiceRace.Snapshot.Phase == ChooseYourOwnAdventure.Live.RacePhase.Ready
        && practiceRace.Snapshot.RacingDriverProgress == 0 && practiceState.LocalVoteWinner is null,
        "New session retained the previous vote or race");
    Check(await practiceClient.CreateSessionAsync() != freshSession, "New sessions reused an identifier");
    Console.WriteLine("PASS: fresh audience sessions replace stale state and refresh the join URL without connecting in practice.");
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
Check(Cue(ChooseYourOwnAdventure.PresentationSlide.FamiliarRhythm).TalkingPoints.Contains("Start")
    && Cue(ChooseYourOwnAdventure.PresentationSlide.FamiliarRhythm).NextAction.Contains("Champion"),
    "Standalone firmware comparison did not lead from Start to the champion");
Check(Cue(ChooseYourOwnAdventure.PresentationSlide.GameLoop).NextAction.Contains("Take Control"),
    "Character loop should lead into Chapter Two");
presenterState.AdvanceLoopStation();
presenterState.ResetGameLoop();
Check(presenterState.GameLoopStation == ChooseYourOwnAdventure.LoopStation.Input,
    "Entering the champion loop should start at Input");
var controllerState = new ChooseYourOwnAdventure.AdventureState();
Check(!controllerState.PreviousControllerStage(), "Controller walkthrough underflowed");
foreach (var stage in Enum.GetValues<ChooseYourOwnAdventure.Presentation.ControllerStage>())
{
    Check(controllerState.ControllerStage == stage, "Controller walkthrough skipped a step");
    var cue = ChooseYourOwnAdventure.PresenterNotes.PresenterCueFactory.Create(
        ChooseYourOwnAdventure.PresentationSlide.InputPackage, controllerState, readyVote, readyRace, false);
    Check(cue.Beat.Contains($"{(int)stage + 1}/4") && cue.TalkingPoints.Length > 0, "Controller cues did not follow the stage");
    Check(ChooseYourOwnAdventure.Input.PresentationNavigation.ResolveNext(
        ChooseYourOwnAdventure.PresentationSlide.InputPackage, controllerState, readyVote, readyRace, false)
        == ChooseYourOwnAdventure.Input.AdventureAction.Confirm, "Shoulder skipped the controller explanation");
    Check(controllerState.AdvanceControllerStage() == (stage != ChooseYourOwnAdventure.Presentation.ControllerStage.Actions),
        "Controller walkthrough escaped before its last step");
}
for (var i = 0; i < 3; i++) Check(controllerState.PreviousControllerStage(), "Could not reverse the controller walkthrough");
controllerState.AdvanceControllerStage();
controllerState.ResetControllerWalkthrough();
Check(controllerState.ControllerStage == ChooseYourOwnAdventure.Presentation.ControllerStage.Setup, "Controller reentry retained a stage");
Console.WriteLine("PASS: separate Start rhythm cue and four-stage controller discovery/access/event walkthrough.");
var tiedVote = readyVote with { Phase = ChooseYourOwnAdventure.Live.VotePhase.Revealed,
    RacingDriver = 1, Charioteer = 1, SpacePilot = 1 };
Check(Cue(ChooseYourOwnAdventure.PresentationSlide.ChooseChampion, tiedVote).NextAction.Contains("Reopen voting"),
    "Tied-vote notes incorrectly continued to a champion");
Check(Cue(ChooseYourOwnAdventure.PresentationSlide.AudienceRace,
    raceSnapshot: readyRace with { Phase = ChooseYourOwnAdventure.Live.RacePhase.Finished,
    Winner = ChooseYourOwnAdventure.Champion.SpacePilot }).NextAction.StartsWith("Open the podium"),
    "Finished-race navigation skipped the podium");
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

// Boost rules: one mutable vote per existing participant; both modes require equal effort.
foreach (var mode in Enum.GetValues<BoostMode>())
{
    var rules = new VoteStore();
    const string rulesSession = "RULES";
    rules.OpenVoting(rulesSession);
    for (var i = 0; i < 20; i++) rules.CastVote(rulesSession, $"r{i}", Champion.RacingDriver);
    rules.CloseVoting(rulesSession);
    rules.OpenBoostVoting(rulesSession);
    rules.CastBoostVote(rulesSession, "unknown", mode);
    Check(rules.GetRaceSnapshot(rulesSession).SteadyVotes + rules.GetRaceSnapshot(rulesSession).BurstVotes == 0,
        "Unregistered spectator voted for race rules");
    rules.CastBoostVote(rulesSession, "r0", BoostMode.Steady);
    rules.CastBoostVote(rulesSession, "r0", mode);
    Check(rules.GetRaceSnapshot(rulesSession).SteadyVotes + rules.GetRaceSnapshot(rulesSession).BurstVotes == 1,
        "Rule vote counted one participant twice");
    Check(rules.StartRace(rulesSession).Phase == RacePhase.Ready, "Race started while rule voting was open");
    var choice = rules.CloseBoostVoting(rulesSession);
    Check(choice.Mode == mode, "Rule majority did not select the chosen mode");
    rules.CastBoostVote(rulesSession, "r1", (BoostMode)999);
    rules.StartRace(rulesSession);
    rules.OpenBoostVoting(rulesSession);
    Check(rules.GetRaceSnapshot(rulesSession).BoostVotePhase == VotePhase.Closed, "Race mode changed during racing");
    // 20 people, 3 taps/sec: at 6 taps each (2 sec), burst mode has charge but no movement.
    for (var i = 0; i < 20; i++) rules.Boost(rulesSession, $"r{i}", 6);
    var partial = rules.GetRaceSnapshot(rulesSession);
    Check(mode == BoostMode.Steady ? partial.RacingDriverProgress > 0
        : partial.RacingDriverProgress == 0 && partial.RacingDriverCharge > 0,
        "Charge mode moved before releasing, or steady mode failed to move");
    for (var i = 0; i < 20; i++) rules.Boost(rulesSession, $"r{i}", 15);
    Check(Math.Abs(rules.GetRaceSnapshot(rulesSession).RacingDriverProgress - .25) < 1e-8,
        "First quarter-lap release did not preserve equal effort");
    // Finish the remaining 63 taps per person; mixed batch sizes cross burst boundaries.
    for (var i = 0; i < 20; i++)
    foreach (var taps in new[] { 20, 20, 20, 3 }) rules.Boost(rulesSession, $"r{i}", taps);
    Check(rules.GetRaceSnapshot(rulesSession).Phase == RacePhase.Finished
        && rules.GetRaceSnapshot(rulesSession).RacingDriverProgress == 1,
        "1680 taps from a 20-person team did not finish in both modes");
    var resetRules = rules.ResetRace(rulesSession);
    Check(resetRules.Mode == mode && resetRules.RacingDriverCharge == 0 && resetRules.RacingDriverProgress == 0,
        "Race reset lost the selected rule or kept charge");
    Check(rules.Reset(rulesSession).Total == 0 && rules.GetRaceSnapshot(rulesSession).Mode == BoostMode.Steady,
        "Session reset retained the previous rule");
}
var tieRules = new VoteStore();
tieRules.OpenVoting("TIE-RULES");
tieRules.CastVote("TIE-RULES", "a", Champion.SpacePilot);
tieRules.CastVote("TIE-RULES", "b", Champion.Charioteer);
tieRules.CloseVoting("TIE-RULES");
tieRules.OpenBoostVoting("TIE-RULES");
tieRules.CastBoostVote("TIE-RULES", "a", BoostMode.ChargeAndRelease);
tieRules.CastBoostVote("TIE-RULES", "b", BoostMode.Steady);
Check(tieRules.CloseBoostVoting("TIE-RULES").Mode == BoostMode.Steady, "Tied rule vote did not use advertised steady fallback");
Console.WriteLine("PASS: boost vote identity, phase guards, tie fallback, charge release and equal effort with 20-person teams.");

// The bonus round snapshots original teams and assigns stable, balanced power sides.
var bonusStore = new SteeringRaceStore();
var bonusPlayers = new Dictionary<string, Champion>
{
    ["a"] = Champion.RacingDriver, ["b"] = Champion.RacingDriver,
    ["c"] = Champion.RacingDriver, ["d"] = Champion.RacingDriver,
    ["e"] = Champion.RacingDriver, ["solo"] = Champion.SpacePilot
};
var prepared = bonusStore.Prepare("BONUS", bonusPlayers);
Check(prepared.Phase == SteeringPhase.Ready && prepared.Vehicles[0].LeftPlayers == 3
    && prepared.Vehicles[0].RightPlayers == 2, "Bonus teams were not balanced");
Check(bonusStore.SideFor("BONUS", prepared.RoundId, "a") == PowerSide.Left
    && bonusStore.SideFor("BONUS", prepared.RoundId, "b") == PowerSide.Right
    && bonusStore.SideFor("BONUS", prepared.RoundId, "solo") == PowerSide.Both,
    "Bonus assignment or solo fallback incorrect");
bonusStore.Power("BONUS", prepared.RoundId, "a", PowerSide.Left, 20);
Check(bonusStore.Get("BONUS").Vehicles[0].LeftPower == 0, "Power applied before start");
bonusStore.Start("BONUS");
bonusStore.Power("BONUS", prepared.RoundId, "a", PowerSide.Right, 20);
bonusStore.Power("BONUS", prepared.RoundId, "unknown", PowerSide.Right, 20);
Check(bonusStore.Get("BONUS").Vehicles[0].RightPower == 0, "Wrong side or unregistered player supplied power");
bonusStore.Power("BONUS", prepared.RoundId, "a", PowerSide.Left, 20);
Check(bonusStore.Step("BONUS", .1)!.Vehicles[0].Offset < 0, "Differential power did not steer");
var replacement = bonusStore.Prepare("BONUS", bonusPlayers);
bonusStore.Start("BONUS");
bonusStore.Power("BONUS", prepared.RoundId, "a", PowerSide.Left, 20);
Check(bonusStore.Get("BONUS").Vehicles[0].LeftPower == 0, "Old-round taps leaked after reset");
Check(bonusStore.SideFor("BONUS", replacement.RoundId, "a") == PowerSide.Left, "Assignment changed on reconnect/reset");
var presenterBonus = new ChooseYourOwnAdventure.Live.LiveSteeringState();
presenterBonus.Update(replacement);
presenterBonus.Update(bonusStore.Stop("BONUS"));
presenterBonus.Update(replacement with { Phase = SteeringPhase.Racing });
Check(presenterBonus.Snapshot.Phase == SteeringPhase.Inactive, "Late tick reopened a stopped bonus round");
Check(bonusStore.Step("BONUS", .1) is null, "Stopped bonus round kept simulating");
// A simple feedback driver must be able to steer a complete lap, without touching main race state.
var driving = new SteeringRaceStore();
var driveRound = driving.Prepare("DRIVE", new Dictionary<string, Champion> { ["driver"] = Champion.RacingDriver });
driving.Start("DRIVE");
for (var tick = 0; tick < 3600 && driving.Get("DRIVE").Phase != SteeringPhase.Finished; tick++)
{
    var vehicle = driving.Get("DRIVE").Vehicles.Single();
    if (tick % 4 == 0)
    {
        var error = vehicle.TargetOffset - vehicle.Offset;
        if (error < .08) driving.Power("DRIVE", driveRound.RoundId, "driver", PowerSide.Left, 1);
        if (error > -.08) driving.Power("DRIVE", driveRound.RoundId, "driver", PowerSide.Right, 1);
    }
    driving.Step("DRIVE", .05);
}
Check(driving.Get("DRIVE").Phase == SteeringPhase.Finished && driving.Get("DRIVE").Winner == 20,
    "Feedback steering could not finish the bonus course");
Console.WriteLine("PASS: bonus side balancing, solo fallback, role/round guards, reset, late snapshots and a complete steered lap.");

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
    try { await clients[0].InvokeAsync("OpenBoostVoting", session, "wrong-key"); throw new Exception("Bad rule key accepted"); }
    catch (Microsoft.AspNetCore.SignalR.HubException) { }
    await presenter.InvokeAsync("ResetRace", session, key);
    await presenter.InvokeAsync("OpenBoostVoting", session, key);
    await clients[0].InvokeAsync("VoteBoostMode", session, "p0", "ChargeAndRelease");
    await clients[1].InvokeAsync("VoteBoostMode", session, "p1", "ChargeAndRelease");
    await clients[2].InvokeAsync("VoteBoostMode", session, "p2", "Steady");
    await presenter.InvokeAsync("CloseBoostVoting", session, key);
    await Until(() => HasRace(x => x.BoostVotePhase == VotePhase.Closed && x.Mode == BoostMode.ChargeAndRelease
        && x.BurstVotes == 2 && x.SteadyVotes == 1), "Live boost vote did not reach clients");
    await clients[2].StopAsync();
    await clients[2].StartAsync();
    await clients[2].InvokeAsync("JoinSession", session);
    await presenter.InvokeAsync("StartRace", session, key);
    await clients[2].InvokeAsync("Boost", session, "p2", 20);
    await Until(() => HasRace(x => x.Mode == BoostMode.ChargeAndRelease && x.SpacePilotProgress == 0
        && x.SpacePilotCharge > .9), "Live charge meter did not fill before movement");
    foreach (var taps in new[] { 20, 20, 20, 4 }) await clients[2].InvokeAsync("Boost", session, "p2", taps);
    await Until(() => HasRace(x => x.Mode == BoostMode.ChargeAndRelease && x.Phase == RacePhase.Finished
        && x.Winner == Champion.SpacePilot), "Live burst race did not finish");
    Console.WriteLine("PASS: live boost voting authorization, result broadcast, reconnect, charge meter and burst finish.");
    var bonusSnapshots = new List<SteeringSnapshot>();
    clients[0].On<SteeringSnapshot>("SteeringUpdated", x => { lock (bonusSnapshots) bonusSnapshots.Add(x); });
    bool HasBonus(Func<SteeringSnapshot, bool> predicate) { lock (bonusSnapshots) return bonusSnapshots.Any(predicate); }
    try { await clients[0].InvokeAsync("PrepareSteeringRace", session, "wrong-key"); throw new Exception("Bad bonus key accepted"); }
    catch (Microsoft.AspNetCore.SignalR.HubException) { }
    await presenter.InvokeAsync("PrepareSteeringRace", session, key);
    await Until(() => HasBonus(x => x.Phase == SteeringPhase.Ready && x.Vehicles.Length == 3), "Bonus preparation not broadcast");
    SteeringSnapshot liveBonus;
    lock (bonusSnapshots) liveBonus = bonusSnapshots.Last(x => x.Phase == SteeringPhase.Ready);
    var assigned = await clients[0].InvokeAsync<PowerSide>("SteeringSide", session, liveBonus.RoundId, "p0");
    Check(assigned is PowerSide.Left or PowerSide.Right, "Live power assignment missing");
    await clients[0].StopAsync();
    await clients[0].StartAsync();
    await clients[0].InvokeAsync("JoinSession", session);
    Check(await clients[0].InvokeAsync<PowerSide>("SteeringSide", session, liveBonus.RoundId, "p0") == assigned,
        "Live reconnect changed the power assignment");
    await presenter.InvokeAsync("StartSteeringRace", session, key);
    var otherSide = assigned == PowerSide.Left ? PowerSide.Right : PowerSide.Left;
    await clients[0].InvokeAsync("SteeringPower", session, liveBonus.RoundId, "p0", (int)otherSide, 20);
    await clients[0].InvokeAsync("SteeringPower", session, liveBonus.RoundId, "p0", (int)assigned, 20);
    await Until(() => HasBonus(x => x.Phase == SteeringPhase.Racing && x.Vehicles.Any(v => v.Champion == 20 && v.Progress > 0)),
        "Server loop did not move the bonus vehicle");
    await presenter.InvokeAsync("StopSteeringRace", session, key);
    await Until(() => HasBonus(x => x.Phase == SteeringPhase.Inactive), "Bonus exit did not restore normal phone state");
    int beforeJoin;
    lock (races) beforeJoin = races.Count;
    await clients[0].InvokeAsync("JoinSession", session);
    await Until(() => { lock (races) return races.Count > beforeJoin; }, "Main race not restored on rejoin");
    RaceSnapshot originalResult;
    lock (races) originalResult = races.Last();
    Check(originalResult.Mode == BoostMode.ChargeAndRelease && originalResult.Phase == RacePhase.Finished
        && originalResult.Winner == Champion.SpacePilot, "Bonus round overwrote the main race result");
    Console.WriteLine("PASS: live bonus preparation, presenter authorization, stable reconnect role, power, server ticks and stop.");
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
