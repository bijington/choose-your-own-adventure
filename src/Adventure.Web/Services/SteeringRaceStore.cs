using System.Collections.Concurrent;
using Adventure.Contracts;
using Adventure.Web.Models;

namespace Adventure.Web.Services;

// An independent bonus round: it never changes the champion vote or main race results.
public sealed class SteeringRaceStore
{
    private sealed class Team(int champion, int left, int right)
    {
        public int Champion = champion, LeftPlayers = left, RightPlayers = right;
        public double Progress, Offset, LeftPower, RightPower;
    }
    private sealed class Round(string id)
    {
        public readonly object Gate = new();
        public string Id = id;
        public SteeringPhase Phase = SteeringPhase.Ready;
        public Dictionary<string, (int Champion, PowerSide Side)> Players = new();
        public List<Team> Teams = new();
        public int? Winner;
    }
    private readonly ConcurrentDictionary<string, Round> _rounds = new(StringComparer.OrdinalIgnoreCase);
    private static string Key(string session) => session.Trim().ToUpperInvariant();
    public SteeringSnapshot Get(string session)
    {
        if (!_rounds.TryGetValue(Key(session), out var round))
            return new(Key(session), "", SteeringPhase.Inactive, [], null);
        lock (round.Gate) return Snapshot(session, round);
    }
    public SteeringSnapshot Prepare(string session, IReadOnlyDictionary<string, Champion> participants)
    {
        var round = new Round(Guid.NewGuid().ToString("N"));
        foreach (var team in participants.GroupBy(x => x.Value).OrderBy(x => x.Key))
        {
            var players = team.OrderBy(x => x.Key, StringComparer.Ordinal).ToArray();
            for (var i = 0; i < players.Length; i++)
                round.Players[players[i].Key] = ((int)team.Key,
                    players.Length == 1 ? PowerSide.Both : i % 2 == 0 ? PowerSide.Left : PowerSide.Right);
            round.Teams.Add(new Team((int)team.Key, (players.Length + 1) / 2, Math.Max(1, players.Length / 2)));
        }
        _rounds[Key(session)] = round;
        return Snapshot(session, round);
    }
    public PowerSide SideFor(string session, string roundId, string participant)
    {
        if (!_rounds.TryGetValue(Key(session), out var round)) return PowerSide.None;
        lock (round.Gate)
            return round.Id == roundId && round.Players.TryGetValue(participant, out var player) ? player.Side : PowerSide.None;
    }
    public SteeringSnapshot Start(string session)
    {
        if (!_rounds.TryGetValue(Key(session), out var round)) return Get(session);
        lock (round.Gate)
        {
            if (round.Phase == SteeringPhase.Ready && round.Teams.Count > 0) round.Phase = SteeringPhase.Racing;
            return Snapshot(session, round);
        }
    }
    public SteeringSnapshot Stop(string session)
    {
        _rounds.TryRemove(Key(session), out var stopped);
        return new SteeringSnapshot(Key(session), stopped?.Id ?? "", SteeringPhase.Inactive, [], null);
    }
    public void Power(string session, string roundId, string participant, PowerSide side, int taps)
    {
        if (side is not (PowerSide.Left or PowerSide.Right) || taps <= 0
            || !_rounds.TryGetValue(Key(session), out var round)) return;
        lock (round.Gate)
        {
            if (round.Id != roundId || round.Phase != SteeringPhase.Racing
                || !round.Players.TryGetValue(participant, out var player)
                || (player.Side != PowerSide.Both && player.Side != side)) return;
            var team = round.Teams.First(x => x.Champion == player.Champion);
            var amount = Math.Min(taps, 20) * .65 / (side == PowerSide.Left ? team.LeftPlayers : team.RightPlayers);
            if (side == PowerSide.Left) team.LeftPower = Math.Min(1, team.LeftPower + amount);
            else team.RightPower = Math.Min(1, team.RightPower + amount);
        }
    }
    public SteeringSnapshot? Step(string session, double seconds)
    {
        if (!_rounds.TryGetValue(Key(session), out var round)) return null;
        lock (round.Gate)
        {
            if (round.Phase != SteeringPhase.Racing) return null;
            var dt = Math.Clamp(seconds, 0, .25);
            var finishes = new List<(Team Team, double Time)>();
            foreach (var team in round.Teams)
            {
                // Differential power steers; the moving course asks for changing balance.
                team.Offset = Math.Clamp(team.Offset + ((team.RightPower - team.LeftPower) * 1.8 - team.Offset * .08) * dt, -1.7, 1.7);
                var error = Math.Abs(team.Offset - Target(team.Progress));
                var grip = error <= .5 ? 1d : .18d;
                var speed = (team.LeftPower + team.RightPower) * .024 * grip;
                var previous = team.Progress;
                team.Progress = Math.Min(1, team.Progress + speed * dt);
                team.LeftPower *= Math.Exp(-dt * 1.6);
                team.RightPower *= Math.Exp(-dt * 1.6);
                if (team.Progress >= 1 && speed > 0) finishes.Add((team, (1 - previous) / speed));
            }
            // Compare crossing times inside the tick, rather than favouring enum order.
            if (finishes.Count > 0)
            {
                round.Winner = finishes.OrderBy(x => x.Time).ThenBy(x => x.Team.Champion).First().Team.Champion;
                round.Phase = SteeringPhase.Finished;
            }
            return Snapshot(session, round);
        }
    }
    public string[] ActiveSessions => _rounds.Keys.ToArray();
    public static double Target(double progress) => .8 * Math.Sin(progress * Math.PI * 4);
    private static SteeringSnapshot Snapshot(string session, Round round) => new(Key(session), round.Id, round.Phase,
        round.Teams.Select(x => new SteeringVehicle(x.Champion, x.Progress, x.Offset, Target(x.Progress),
            x.LeftPower, x.RightPower, x.LeftPlayers, x.RightPlayers)).ToArray(), round.Winner);
}
