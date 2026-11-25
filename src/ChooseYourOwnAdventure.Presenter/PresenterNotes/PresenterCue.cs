using ChooseYourOwnAdventure.Live;
using ChooseYourOwnAdventure.Presentation;

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
        PresentationSlide.OrbitOverview => "Building a Game in MAUI",
        PresentationSlide.InstallOrbit => "Install & Register Orbit",
        PresentationSlide.GameSceneView => "Give Orbit Somewhere to Render",
        PresentationSlide.LoadGameScene => "Load a Game Scene",
        PresentationSlide.ChampionChapter => "Meet Our Champion",
        PresentationSlide.ChooseChampion => "Choose Your Champion",
        PresentationSlide.ChosenChampion => "Our Champion",
        PresentationSlide.FamiliarRhythm => "A Familiar Rhythm",
        PresentationSlide.GameLoop => "A Frame in the Life of Our Champion",
        PresentationSlide.InputChapter => "Take Control",
        PresentationSlide.InputOverview => "Many Ways to Play",
        PresentationSlide.MauiInput => "MAUI Already Has Input",
        PresentationSlide.InputPackage => "Connect a Game Controller",
        PresentationSlide.VehicleControl => "Drive Our Champion",
        PresentationSlide.BoostChoice => "Choose Our Route",
        PresentationSlide.SignalRChapter => "An Adventure Together",
        PresentationSlide.SignalRTransition => "What If the Input Source Changes?",
        PresentationSlide.SignalRArchitecture => "The Room Becomes the Input",
        PresentationSlide.AudienceRace => "The Audience Grand Prix",
        PresentationSlide.Podium => "The Podium",
        PresentationSlide.EpilogueChapter => "The Journey Home",
        PresentationSlide.MauiAnimations => "MAUI Already Has Some Magic",
        PresentationSlide.FutureIdeas => "What Would We Add Next?",
        PresentationSlide.Recap => "The End · Credits",
        PresentationSlide.AfterCreditsRace => "After Credits · Team Steering (Optional)",
        _ => slide.ToString()
    };

    public static PresenterCue Create(PresentationSlide slide, AdventureState state,
        VoteSnapshot vote, RaceSnapshot race, bool connected)
    {
        const string back = "Left shoulder / Page Up / left page: back.";
        const string forward = "Right shoulder / Page Down / South / Enter / right page: continue. ";
        switch (slide)
        {
            case PresentationSlide.Title:
                return new("Cover · opening story",
                    """
                    Good afternoon, thank you for joining me for the final session of the day.

                    My name is Shaun Lawrence and I am hoping to take you on an adventure into building games with .NET MAUI.

                    Does anyone remember the Choose Your Own Adventure books where you put a finger between the pages because you weren’t completely convinced that opening the mysterious door was a good idea?

                    Well todays talk is inspired by those plus the fact that as I age in life I become worse at making decisions, so today you get to. You’ll choose our champion, shape the example we explore, and eventually become the input that powers the final challenge.

                    We’ll start with ordinary MAUI, give our champion a world and some movement, and then connect this screen to your phones. The technology won’t just be something we discuss. It will be running the session.
                    """,
                    "South / Enter or tap anywhere on the cover: begin.", "Open the first decision.");
            case PresentationSlide.FirstDecision:
                return new("The practice decision",
                    """
                    Before we do anything technical, let's start with a quick decision, we are at the end of the day nearing the after party...

                    We can begin our adventure… or, we could try going straight to the after-party.

                    What's that I hear you all say? You need a drink, OK let's go.,
                    """,
                    "South / Enter or upper card: the talk. East / Shift or lower card: page 404. " + back,
                    "Choose the talk, or take the page-404 detour and rejoin the prologue.");
            case PresentationSlide.AfterParty:
                return new("After-party not found", "Pause for the joke. Then: ‘Please complete this adventure first.’",
                    "South / Enter / right page: prologue. " + back, "Begin Discovering Orbit.");
            case PresentationSlide.PrologueChapter:
                return new("Prologue · Discovering Orbit",
                    "Every adventure needs a world to happen in. Ours happens inside a MAUI app. We’re adding another way to make part of that app feel alive.",
                    forward + back, "Introduce the pieces that make up a game in MAUI.");
            case PresentationSlide.OrbitOverview:
                var overview = OrbitOverviewWalkthrough.ForStage(state.OrbitOverviewStage);
                var overviewNext = state.OrbitOverviewStage == OrbitOverviewStage.Game
                    ? "Show how to install and register Orbit."
                    : "Explain: " + OrbitOverviewWalkthrough.ForStage(state.OrbitOverviewStage + 1).Component + ".";
                return new($"Building a game in MAUI · {(int)state.OrbitOverviewStage + 1}/4 · {overview.Component}",
                    overview.TalkingPoints,
                    "South / Right shoulder / Enter / Page Down / right page: next building block. " +
                    "Left shoulder / East / Page Up / Escape / left page: previous; from step 1, previous spread.", overviewNext);
            case PresentationSlide.InstallOrbit:
                return new("Install & Register Orbit",
                    "Orbit arrives as NuGet packages and MAUI builder extensions. Install Bijington.Orbit.Engine and register it with UseOrbitEngine(). This is still a normal MAUI project with no special editor or runtime required. Keep this short.",
                    forward + back, "Give the engine somewhere inside the MAUI UI to render.");
            case PresentationSlide.GameSceneView:
                return new("GameSceneView · a place to render",
                    "GameSceneView is a MAUI view in a MAUI page. Orbit renders into it.",
                    forward + back, "Load a scene into the view.");
            case PresentationSlide.LoadGameScene:
                return new("Load a Game Scene",
                    "A GameScene represents the current part of the game. Load the opening scene into GameView and start the scene manager. This presentation uses a scene for each major beat, registered and ordered by PresentationDeck.",
                    forward + back, "Show what Start() does: the update/render rhythm.");
            case PresentationSlide.ChampionChapter:
                return new("Chapter One · Meet Our Champion",
                    "Introduce Meet Our Champion. Let the chapter opener land, then invite the room to choose our main character.",
                    forward + back, "Open the champion vote slide.");
            case PresentationSlide.ChooseChampion:
                var local = !connected || state.LocalVoteWinner is not null;
                var phase = state.LocalVoteWinner is not null ? VotePhase.Revealed : vote.Phase;
                var tie = !local && state.LocalVoteWinner is null && phase == VotePhase.Revealed && vote.RevealedChampion is null;
                var next = local
                    ? state.LocalVoteWinner is null ? "Confirm the highlighted champion locally." : "After the reveal, continue with the local champion."
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
                    (local ? "Left stick Up/Down or arrow keys: select. " : string.Empty) + "South / Enter / right page: " + next + " East / Shift / Escape: reset. " + back, next);
            case PresentationSlide.ChosenChampion:
                if (state.ChampionRenderingStage is not { } renderStage)
                    return new("Chosen champion · summary", "Say: ‘You’ve chosen more than our hero — you’ve chosen the physics we’ll have to live with.’ Name the chosen challenge: driver = precision and braking; charioteer = momentum; pilot = gravity and thrust. Then explore their backstory: from humble beginnings as a dot to an animated sprite.",
                        "South / Enter / Right shoulder / Page Down / right page: begin with a dot. " + back,
                        "Begin our champion’s four-stage origin story.");
                var rendering = ChampionRenderingWalkthrough.ForStage(renderStage);
                return new($"Our champion’s origins · {(int)renderStage + 1}/4 · {rendering.Title}", rendering.TalkingPoints,
                    "South / Enter / Right shoulder / Page Down / right page: next stage. Left shoulder / East / Page Up / Escape / left page: previous stage; before the dot, the champion summary.",
                    renderStage == ChampionRenderingStage.Sprite ? "Follow a frame in the life of our champion."
                        : "Show: " + ChampionRenderingWalkthrough.ForStage(renderStage + 1).Title + ".");
            case PresentationSlide.FamiliarRhythm:
                return new("Start() · a familiar rhythm",
                    "Start begins the engine’s loop. If you’ve worked with firmware, the shape is familiar: read inputs, evolve state, write outputs, wait, repeat. The snippets are conceptual; we do not write a blocking while loop in our MAUI page. Orbit runs the loop and our GameObjects provide Update and Render. Now we need a champion to put inside that world.",
                    forward + back, "Meet Our Champion, then let the audience choose.");
            case PresentationSlide.GameLoop:
                return new($"Game loop · {state.GameLoopStation.ToString().ToUpperInvariant()}",
                    "Champion thought: “" + ChampionLoopStory.For(state.Champion, state.GameLoopStation).Thought + "”\n" +
                    ChampionLoopStory.For(state.Champion, state.GameLoopStation).Explanation +
                    "\nThis is the rhythm we met when starting the scene, now applied to our chosen champion.",
                    "South / Enter / right page: next station. Left / East / Escape: previous station; before Input, champion origins. Left shoulder / Page Up: previous spread. Right shoulder / Page Down or NEXT: Take Control.",
                    "Step around INPUT → UPDATE → RENDER → WAIT, then open Chapter Two: Take Control.");
            case PresentationSlide.InputChapter:
                return new("Chapter Two · Take Control",
                    "We know how their world runs. Now let’s put the controls in your hands.",
                    forward + back, "Introduce the different ways to play.");
            case PresentationSlide.InputOverview:
                return new("Input possibilities · 30–45 seconds",
                    "There are more ways into our game than a button. MAUI supplies touch, mouse and gestures, plus device sensor APIs. Accelerometers measure acceleration and can detect shake; a gyroscope measures rotation speed, while orientation and compass readings can support attitude or heading-based ideas. Imagine tilting to steer, turning to aim or shaking to act.\n" +
                    "These depend on device support. We’re describing possibilities, not activating sensors here. The right page introduces keyboard and game controllers through Orbit.Input’s shared game-input layer. Start with the simplest source: a normal MAUI button.",
                    "South / Enter / Right shoulder / Page Down / right page: MAUI button example. " + back,
                    "Show how a press and release becomes intent.");
            case PresentationSlide.MauiInput:
                return new("Touch and mouse · ordinary MAUI UI",
                    "Start with what MAUI already gives us: a Button works with touch and mouse. Clicked or Command can represent a single action; Pressed and Released are useful for held controls.\n" +
                    "The XAML and handlers are a teaching example. They feed our existing AdventureInputRouter: press sets intent, release clears it. The handler does not move or redraw the champion; Update and Render still do that.\n" +
                    "Gestures and pointer events can support richer interactions. Orbit.Input is the next option when we want keyboard and game controllers; it does not replace normal MAUI UI.",
                    "South / Enter / Right shoulder / Page Down / right page: keyboard and game-controller input. " + back,
                    "Introduce the Orbit.Input package for keyboard and controllers.");
            case PresentationSlide.InputPackage:
                var controller = ControllerWalkthrough.ForStage(state.ControllerStage);
                return new($"Controller · {(int)state.ControllerStage + 1}/4 · {controller.Title}", controller.TalkingPoints,
                    "South / Enter / Right shoulder / Page Down / right page: next step. Left shoulder / East / Escape / left page: previous step. " + back,
                    state.ControllerStage == ControllerStage.Actions ? "Put our champion in motion."
                        : "Show: " + ControllerWalkthrough.ForStage(state.ControllerStage + 1).Title + ".");
            case PresentationSlide.VehicleControl:
                var model = state.Champion switch
                {
                    Champion.Charioteer => "Build speed, then release. Momentum persists; only light drag slows the chariot.",
                    Champion.SpacePilot => "Thrust changes velocity. Release and gravity takes over on the next update, with no new input.",
                    _ => "Hold throttle and steer together. Velocity and lateral position are independent pieces of state."
                };
                return new("Live control · " + (state.Champion?.ToString() ?? "RacingDriver"), model + "\nPoint out the active code branch and live values.",
                    "South / Enter: throttle, pull or thrust. East / Shift / Down: brake or down-thrust. Left/Right: driver steering. Right shoulder / Page Down / lower-right NEXT: next chapter.",
                    "Ask: ‘What if the input source changes?’ Then let the audience choose our route.");
            case PresentationSlide.BoostChoice:
                return new("Second decision · which route?",
                    "The finish lies beyond the mountains. Which way do we go: follow the winding road, or take the ancient launch pads? The road rewards steady tapping. The pads need the team to charge together, then automatically leap forward. Same team-balanced effort; ties take the road.\nOn choosing the pads: ‘Excellent. I’m sure somebody maintained them.’\n" +
                    "Once chosen: ‘To make that decision together, we connected your phones to our game. Let’s see how that connection works.’",
                    "South / Enter: open vote, close and reveal, then continue. East: reopen the route vote before racing. Offline: Up/Down or right-page options choose locally; confirm again to continue. " + back,
                    race.BoostVotePhase == VotePhase.Closed ? "Open An Adventure Together." : "Let the audience choose our route to the finish.");
            case PresentationSlide.SignalRChapter:
                return new("Chapter Three · An Adventure Together",
                    "One champion. An entire room of players. Let’s turn the audience into the input.",
                    forward + back, "Ask what happens when the input source changes.");
            case PresentationSlide.SignalRTransition:
                return new("From one controller to the room",
                    "Ask: ‘What if the input came from all of you?’ The game still consumes intent; the source changes.\n" +
                    "The audience already used this route for the champion vote. Now reveal how phones, the server and the presentation share the experience.",
                    "South / Enter / right page: follow one vote. " + back,
                    "Explain SignalR, then follow a vote from phone to screen.");
            case PresentationSlide.SignalRArchitecture:
                var step = SignalRWalkthrough.ForStage(state.SignalRStage);
                var nextStep = state.SignalRStage == SignalRStage.Presenter
                    ? "Open the audience race."
                    : "Show: " + SignalRWalkthrough.ForStage(state.SignalRStage + 1).Title + ".";
                var concept = state.SignalRStage == SignalRStage.Browser
                    ? "SignalR is ASP.NET Core’s library for real-time communication. The server can push updates to connected clients without each client repeatedly asking for news.\n" +
                      "Think of the hub as the room’s switchboard: phones send intent; our server stores the shared state and broadcasts an update to phones and the MAUI app. SignalR carries messages; Orbit renders the result.\n" +
                      "Let’s follow one vote from your phone to this screen.\n\n"
                    : string.Empty;
                return new($"SignalR · {(int)state.SignalRStage + 1}/4 · {step.Title}", concept + step.TalkingPoints,
                    "South / Right shoulder / Enter / Page Down / right page: next step. " +
                    "Left shoulder / East / Page Up / Escape / left page: previous step; from step 1, previous spread.", nextStep);
            case PresentationSlide.AudienceRace:
                if (state.IsRehearsing)
                    return new("Practice · skip the audience race", "Describe the architecture or skip ahead. No race-start or reset command is sent in practice mode.",
                        "Right page / South / Enter / Right shoulder: skip to the podium. " + back,
                        "Skip the live race and continue the ending.");
                return new($"Audience race · {race.Phase}",
                    (race.Mode == BoostMode.ChargeAndRelease
                        ? "You chose the ancient launch pads. Tap together to charge each pad; full meters launch automatically. Watch for JUMP!\n"
                        : "You chose the winding road. Every tap keeps your original champion moving.\n") +
                    (race.BoostVotePhase == VotePhase.Closed
                        ? "You chose your champion. You chose our route. Now let’s find out whether either was a sensible decision.\n"
                        : "You chose your champion. Now let’s find out whether that was a sensible decision.\n") +
                    "Your original vote chose your team. Now we’re competing, rather than making one shared decision.\n" +
                    "Before starting, ask: ‘What if one champion has twice as many people? Have we built a race — or a headcount?’ Pause for answers. The design bug is rewarding headcount; divide tap strength by team size so average effort decides progress. The fix is already implemented.\n" +
                    "Phones send boost intent. The server owns progress and normalizes boost by team size.\n" +
                    (race.Phase == RacePhase.Finished ? "Celebrate the winner on the podium." : "Remind everyone that they race for their original champion. Keep the QR code available for reconnects."),
                    "South / Enter: start race, or continue after the finish. East / Shift / Escape: reset race.",
                    race.Phase == RacePhase.Finished ? "Open the podium, then The Journey Home." : "Start the race, then encourage the audience to boost.");
            case PresentationSlide.Podium:
                return new("The Podium · celebrate the teams",
                    "Call out the winning team and thank the other teams. Keep it short and energetic, and give the audience a moment to react.",
                    forward + back, "Let’s take some of that life back into our everyday applications.");
            case PresentationSlide.EpilogueChapter:
                return new("Epilogue · The Journey Home",
                    "Every race has a finish. Every adventure leaves us with something to take home.",
                    forward + back, "Show the MAUI animation demo.");
            case PresentationSlide.MauiAnimations:
                return new("MAUI animations · a little magic",
                    "Start The Journey Home with what the audience can take back to everyday MAUI apps. These are real XAML controls animated with MAUI’s Animation API: translation, rotation, scale and opacity. Easing changes the feel of the motion.\n" +
                    "Tap Play it again if useful. These are great for transitions, feedback and playful UI. These effects also work in ordinary apps, without a game loop. Keep this demonstration to about 30 seconds.",
                    "Tap Play it again to replay. South / Enter / Right shoulder / Page Down: what would we add next? " + back,
                    "Explore haptics, audio and visual feedback.");
            case PresentationSlide.FutureIdeas:
                return new("Future ideas · 60–90 seconds",
                    "Ask how players know their boost landed. A short haptic pulse can acknowledge touch on supported devices. " +
                    "A countdown sound lets players keep their eyes on the race. A boost animation and visible countdown help when sound and vibration are off. " +
                    "Use confirmed events for shared cues such as the finish.",
                    "Right shoulder / Page Down / South / Enter / right page: continue. " + back,
                    "Turn to The End for the credits, recap and resources.");
            case PresentationSlide.Recap:
                return new("Closing credits · resources",
                    """
                    We started with a normal MAUI application and a canvas.
                    Orbit gave us scenes, game objects and the Update/Render loop. Input let us control that world.
                    SignalR let the entire room become the input source.
                    The input source changed. The game loop stayed the same.

                    If the resources QR code is visible, invite the audience to scan it for the post, code and further resources.
                    """,
                    "Leave this final spread visible for questions and the resources link. " + back,
                    "I’m Shaun Lawrence. Thank you for choosing your own adventure. Invite questions.");
            case PresentationSlide.AfterCreditsRace:
                return new("Optional encore · keep the competition",
                    "Let the achievement popup land: ‘You stayed after the credits.’ It disappears automatically. Each champion team now has left and right power groups. Check your original phone for your assigned side. Tap together for speed; change the balance to steer the marker into the green course band. Running off course slows you down without erasing progress. One-person teams get both controls.\n" +
                    "Prepare first so everyone can see their role, then start. This is a separate bonus round; the original podium result stays intact. In practice mode this is a briefing only.",
                    "South / Enter: prepare, start, then return after finish. East / Escape: stop and return to the closing page.",
                    "Leave the closing page visible when finished.");
            default:
                return new("Main spread", string.Empty, "Right shoulder / Page Down / South / Enter / right page: continue. " + back,
                    "Turn the page when ready.");
        }
    }
}
