using ChooseYourOwnAdventure.Live;

namespace ChooseYourOwnAdventure.PresenterNotes;

public sealed record PresenterCue(string Beat, string TalkingPoints, string Controls, string NextAction);

public static class PresenterCueFactory
{
    public static string TitleFor(PresentationSlide slide) => slide switch
    {
        PresentationSlide.Title => "Choose Your Own Adventure",
        PresentationSlide.FirstDecision => "Your First Decision",
        PresentationSlide.AfterParty => "After-Party Not Found",
        PresentationSlide.PrologueChapter => "Discovering Orbit",
        PresentationSlide.OrbitOverview => "What Is Orbit?",
        PresentationSlide.InstallOrbit => "Install & Register Orbit",
        PresentationSlide.GameSceneView => "Give Orbit Somewhere to Render",
        PresentationSlide.LoadGameScene => "Load a Game Scene",
        PresentationSlide.ChampionChapter => "Meet Our Champion",
        PresentationSlide.ChooseChampion => "Choose Your Champion",
        PresentationSlide.ChosenChampion => "Our Champion",
        PresentationSlide.InputChapter => "Take Control",
        PresentationSlide.InputPackage => "How Does It Move?",
        PresentationSlide.GameLoop => "The Game Loop",
        PresentationSlide.VehicleControl => "Drive Our Champion",
        PresentationSlide.SignalRChapter => "An Adventure Together",
        PresentationSlide.SignalRTransition => "What If the Input Source Changes?",
        PresentationSlide.SignalRArchitecture => "The Room Becomes the Input",
        PresentationSlide.AudienceRace => "The Audience Grand Prix",
        PresentationSlide.EpilogueChapter => "The Journey Home",
        PresentationSlide.Podium => "The Podium",
        PresentationSlide.Recap => "Adventure Complete",
        _ => slide.ToString()
    };

    public static PresenterCue Create(PresentationSlide slide, AdventureState state,
        VoteSnapshot vote, RaceSnapshot race, bool connected)
    {
        const string back = "Left page / Escape: back.";
        switch (slide)
        {
            case PresentationSlide.Title:
                return new("Cover · opening story", string.Empty,
                    "A / Enter or tap anywhere on the cover: begin.", "Open the first decision.");
            case PresentationSlide.FirstDecision:
                return new("The practice decision",
                    "Let the room shout its preference. This is a joke, not a SignalR vote.\nThe after-party shortcut leads to page 404: ‘Please complete this adventure first.’",
                    "A / Enter or upper card: the talk. B / Shift or lower card: page 404. " + back,
                    "Choose the talk, or take the page-404 detour and rejoin the prologue.");
            case PresentationSlide.AfterParty:
                return new("After-party not found", "Pause for the joke. Then: ‘Please complete this adventure first.’",
                    "A / Enter / right page: prologue. " + back, "Begin Discovering Orbit.");
            case PresentationSlide.ChooseChampion:
                var local = !connected;
                var phase = state.LocalVoteWinner is not null ? VotePhase.Revealed : vote.Phase;
                var tie = state.LocalVoteWinner is null && phase == VotePhase.Revealed && vote.RevealedChampion is null;
                var next = local
                    ? state.LocalVoteWinner is null ? "Select a right-page card to reveal a local champion." : "After the reveal, continue with the local champion."
                    : phase switch
                    {
                        VotePhase.Ready => "Open voting.",
                        VotePhase.Open => "Close voting when the audience has chosen.",
                        VotePhase.Closed => "Wait for the 3-second lock-in, then reveal the winner.",
                        _ => tie ? "Reopen voting; there is no hidden tie-breaker." : "Let the winner enlarge, then continue to the character summary."
                    };
                return new(tie ? "A tie — one more round" : local ? "Local champion selection" : $"Audience vote · {phase}",
                    "The winning vote themes the examples. Each participant’s original vote also decides their race team.\n" +
                    (tie ? "Keep all choices visible and invite another round." : "On reveal, the losing cards fade and the winner grows in place."),
                    "A / Enter / right page: " + next + " B / Shift: reset. " + back, next);
            case PresentationSlide.ChosenChampion:
                return state.ChampionRenderingVisible
                    ? new("Rendering code revealed", "Point to Render(ICanvas, RectF), then the drawing primitives.\nThe champion remains visible: this is the code responsible for drawing it. Scene setup was covered in the prologue.",
                        "A / Enter / right page: Take Control. Left page / Escape: hide code.", "Turn to Take Control.")
                    : new("Chosen champion · summary", "Introduce the chosen name and character once. Let the audience see it before explaining its implementation.",
                        "A / Enter / right page: reveal rendering code on the left. " + back, "Reveal Render() without turning the page.");
            case PresentationSlide.GameLoop:
                return state.GameLoopStage == GameLoopStage.FirmwareComparison
                    ? new("Firmware / superloop comparison", "The same shape appears in firmware: read inputs, evolve state, draw outputs, wait.\nInput affects state. Update evolves state. Render displays state.",
                        "Right / D or lower-right NEXT: driving demo. A / Enter: loop station.", "Continue to the live control demo.")
                    : new($"Game loop · {state.GameLoopStation.ToString().ToUpperInvariant()}",
                        state.GameLoopStation switch
                        {
                            LoopStation.Input => "Read intent. Input affects state; it does not directly draw the world.",
                            LoopStation.Update => "Evolve state using elapsed time: movement, animation, physics and logic.",
                            LoopStation.Render => "Draw the current state. The world can keep updating even when input stops.",
                            _ => "Yield until the next frame, then repeat. This is the continuous game-loop mental model."
                        }, "A / Enter / tap: next station. Left: previous station. Right / D or NEXT: firmware comparison.",
                        "Step around INPUT → UPDATE → RENDER → WAIT, then show the firmware comparison.");
            case PresentationSlide.VehicleControl:
                var model = state.Champion switch
                {
                    Champion.Charioteer => "Build speed, then release. Momentum persists; only light drag slows the chariot.",
                    Champion.SpacePilot => "Thrust changes velocity. Release and gravity takes over on the next update, with no new input.",
                    _ => "Hold throttle and steer together. Velocity and lateral position are independent pieces of state."
                };
                return new("Live control · " + (state.Champion?.ToString() ?? "RacingDriver"), model + "\nPoint out the active code branch and live values.",
                    "A / Enter: throttle, pull or thrust. B / Shift / Down: brake or down-thrust. Left/Right: driver steering. Up / W / lower-right NEXT: next chapter.",
                    "Ask: ‘What if the input source changes?’ Then open An Adventure Together.");
            case PresentationSlide.AudienceRace:
                return new($"Audience race · {race.Phase}",
                    "Phones send boost intent. The server owns progress and normalizes boost by team size.\n" +
                    (race.Phase == RacePhase.Finished ? "Celebrate the winner before turning to The Journey Home." : "Remind everyone that they race for their original champion. Keep the QR code available for reconnects."),
                    "A / Enter: start race, or continue after the finish. B / Shift / Escape: reset race.",
                    race.Phase == RacePhase.Finished ? "Open The Journey Home, then the podium." : "Start the race, then encourage the audience to boost.");
            default:
                return new("Main spread", string.Empty, "A / Enter / right page: continue. B / Shift or " + back,
                    "Turn the page when ready.");
        }
    }
}
