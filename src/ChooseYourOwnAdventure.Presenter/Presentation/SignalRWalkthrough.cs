namespace ChooseYourOwnAdventure.Presentation;

public sealed record SignalRWalkthroughStep(
    string Title, string Component, string CodeTitle, string Code,
    string Caption, string TalkingPoints);

/// <summary>Focused excerpts of the existing champion-vote path, in message order.</summary>
public static class SignalRWalkthrough
{
    public static SignalRWalkthroughStep ForStage(SignalRStage stage) => stage switch
    {
        SignalRStage.Browser => new(
            "The browser sends intent", "AUDIENCE BROWSER", "JavaScript · index.html",
            """
const connection =
  new signalR.HubConnectionBuilder()
    .withUrl('/adventureHub')
    .withAutomaticReconnect().build();

await connection.start();
await connection.invoke(
  'JoinSession', sessionId);

await connection.invoke(
  'Vote', sessionId,
  participantId, selectedChampion);
""",
            "One phone sends a choice, not a vote total.",
            "Follow one champion vote from the opening. The phone is a SignalR client. It connects, joins the session, then invokes Vote when a champion card is tapped.\nThe browser sends identity and intent; it does not decide the winner. These are focused excerpts from index.html; connection setup and the click handler are shown together."),
        SignalRStage.Hub => new(
            "The hub receives the call", "ASP.NET CORE HUB", "C# · AdventureHub",
            """
// Program.cs
app.MapHub<AdventureHub>(
    "/adventureHub");

// AdventureHub : Hub
public async Task Vote(
    string sessionId,
    string participantId,
    string championName)
{
    var normalized = Normalize(sessionId);
    // Validation omitted here.
    // State update + broadcast next...
}
""",
            "invoke('Vote', ...) reaches Vote(...).",
            "Match the URL /adventureHub to MapHub, and the JavaScript name Vote to the public hub method. The arguments cross the boundary in the same order.\nThe actual method normalizes the session and participant and validates the champion enum. The excerpt leaves those checks out to keep the entry point readable. JoinSession adds each connection to its session group."),
        SignalRStage.Broadcast => new(
            "The server shares the result", "STATE + GROUP BROADCAST", "C# · Vote() continued",
            """
var participant =
    NormalizeParticipant(participantId);

var snapshot = _voteStore.CastVote(
    normalized, participant, champion);

await Clients.Group(normalized)
    .SendAsync("VoteUpdated", snapshot);
""",
            "Server-owned state. One snapshot for the session.",
            "CastVote owns the shared state: it only accepts votes while the round is open, stores one choice per participant and returns a snapshot. Its lock protects concurrent updates.\nClients.Group sends VoteUpdated to the session's connections, including the presenter and the audience browsers. This is a server push; the presenter is not polling the server. The snapshot contains totals, phase and any revealed winner."),
        SignalRStage.Presenter => new(
            "MAUI receives; Orbit renders", "MAUI CLIENT + ORBIT", "C# · receive → state → render",
            """
// AudienceVoteClient.StartAsync()
_connection.On<VoteSnapshot>(
    "VoteUpdated", _voteState.Update);

// LiveVoteState.Update() — inside lock
_snapshot = snapshot;

// OpeningScene.DrawChampionSelection()
var votes = CurrentVotes;
// Cards draw totals from this snapshot.
""",
            "One shared decision. Now a roomful of rivals.",
            "VoteUpdated matches the server's event name. The MAUI app is another SignalR client connected to the same hub and joined to the same session.\nThe callback updates LiveVoteState under a lock. OpeningScene reads that state on the normal Orbit loop and draws the totals and reveal animation. SignalR does not draw the scene or start another game loop.\nSo far, we’ve used SignalR to make a decision together. You send a choice, the server updates the state, and everyone sees the result.\n\nBut choosing a champion is the easy part. What if we wanted to pit an entire room against each other… in a race to the death?\n\n[Pause.]\n\nOf your phone batteries, obviously.\n\nNow the input keeps coming. Every tap matters, every team is competing, and everyone needs to see the same race. The architecture stays familiar — but the rules get more interesting.\n\n[Advance to the race before explaining the fairness challenge.]"),
        _ => throw new ArgumentOutOfRangeException(nameof(stage))
    };
}
