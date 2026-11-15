using Adventure.Web.Models;
using Adventure.Web.Services;
using Microsoft.AspNetCore.SignalR.Client;

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
        await demo.InvokeAsync("Boost", "ORBIT", "demo-SpacePilot", 20);
        await demo.InvokeAsync("Boost", "ORBIT", "demo-SpacePilot", 4);
        await demo.InvokeAsync("Boost", "ORBIT", "demo-extra", 20);
        await demo.InvokeAsync("Boost", "ORBIT", "demo-extra", 4);
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
Check(Enum.GetValues<ChooseYourOwnAdventure.PresentationSlide>().Select(x => (int)x).SequenceEqual(Enumerable.Range(1, 18).Append(404)), "Slide numbering mismatch");

var store = new VoteStore();
const string loadSession = "LOAD";
store.OpenVoting(loadSession);
await Task.WhenAll(Enumerable.Range(0, 600).Select(i => Task.Run(() =>
    store.CastVote(loadSession, $"p{i}", Enum.GetValues<Champion>()[i % 3]))));
Check(store.CloseVoting(loadSession).Total == 600, "Concurrent votes lost");
Check(store.RevealWinner(loadSession).RevealedChampion is null, "Tie picked a winner");
store.StartRace(loadSession);
await Task.WhenAll(Enumerable.Range(0, 600).Select(i => Task.Run(() => store.Boost(loadSession, $"p{i}", 12))));
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
    store.Boost("BATCHED", participant, 20);
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
    await clients[2].InvokeAsync("Boost", session, "p2", 12);
    await Until(() => HasRace(x => Math.Abs(x.SpacePilotProgress - .5) < .00001 && x.RacingDriverProgress == 0), "Original race team lost");
    await clients[2].StopAsync();
    await clients[2].StartAsync();
    await clients[2].InvokeAsync("JoinSession", session);
    await clients[2].InvokeAsync("Boost", session, "p2", 12);
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
