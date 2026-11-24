using Adventure.Web.Models;
using System.Collections.Concurrent;

namespace Adventure.Web.Services;

public sealed class VoteStore
{
    // About 28 seconds at three taps per second from each team member.
    private const double AverageTapsPerPlayerToFinish = 84d;

    private sealed class SessionVotes
    {
        public object Gate { get; } = new();
        public Dictionary<string, Champion> VotesByParticipant { get; } = new();
        public VotePhase Phase { get; set; } = VotePhase.Ready;
        public Champion? RevealedChampion { get; set; }

        public RacePhase RacePhase { get; set; } = RacePhase.Ready;
        public Dictionary<Champion, double> RaceProgress { get; } = new()
        {
            [Champion.RacingDriver] = 0d,
            [Champion.Charioteer] = 0d,
            [Champion.SpacePilot] = 0d
        };
        public BoostMode Mode { get; set; }
        public VotePhase BoostVotePhase { get; set; }
        public Dictionary<string, BoostMode> BoostVotes { get; } = new();
        public Dictionary<Champion, double> Charge { get; } = new()
        {
            [Champion.RacingDriver] = 0, [Champion.Charioteer] = 0, [Champion.SpacePilot] = 0
        };
        public Champion? RaceWinner { get; set; }
    }

    private readonly ConcurrentDictionary<string, SessionVotes> _sessions =
        new(StringComparer.OrdinalIgnoreCase);

    public VoteSnapshot CastVote(string sessionId, string participantId, Champion champion)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            if (session.Phase == VotePhase.Open && Enum.IsDefined(champion))
            {
                session.VotesByParticipant[participantId] = champion;
            }

            return Snapshot(sessionId, session);
        }
    }

    public VoteSnapshot OpenVoting(string sessionId)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            session.BoostVotes.Clear();
            session.BoostVotePhase = VotePhase.Ready;
            session.Mode = BoostMode.Steady;
            session.Phase = VotePhase.Open;
            session.RevealedChampion = null;
            ResetRaceUnsafe(session);
            return Snapshot(sessionId, session);
        }
    }

    public VoteSnapshot CloseVoting(string sessionId)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            if (session.Phase == VotePhase.Open)
            {
                session.Phase = VotePhase.Closed;
            }

            return Snapshot(sessionId, session);
        }
    }

    public VoteSnapshot RevealWinner(string sessionId)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            if (session.Phase is VotePhase.Closed or VotePhase.Revealed)
            {
                session.RevealedChampion = GetWinningChampion(session);
                session.Phase = VotePhase.Revealed;
            }

            return Snapshot(sessionId, session);
        }
    }

    public VoteSnapshot Reset(string sessionId)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            session.BoostVotes.Clear();
            session.BoostVotePhase = VotePhase.Ready;
            session.Mode = BoostMode.Steady;
            session.VotesByParticipant.Clear();
            session.Phase = VotePhase.Ready;
            session.RevealedChampion = null;
            ResetRaceUnsafe(session);
            return Snapshot(sessionId, session);
        }
    }

    public VoteSnapshot GetSnapshot(string sessionId)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            return Snapshot(sessionId, session);
        }
    }

    public RaceSnapshot OpenBoostVoting(string sessionId)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            if (session.RacePhase == RacePhase.Ready) session.BoostVotePhase = VotePhase.Open;
            return RaceSnapshotFor(sessionId, session);
        }
    }

    public RaceSnapshot CastBoostVote(string sessionId, string participantId, BoostMode mode)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            if (session.BoostVotePhase == VotePhase.Open && session.RacePhase == RacePhase.Ready
                && session.VotesByParticipant.ContainsKey(participantId) && Enum.IsDefined(mode))
                session.BoostVotes[participantId] = mode;
            return RaceSnapshotFor(sessionId, session);
        }
    }

    public RaceSnapshot CloseBoostVoting(string sessionId)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            if (session.BoostVotePhase == VotePhase.Open && session.RacePhase == RacePhase.Ready)
            {
                session.Mode = session.BoostVotes.Values.Count(x => x == BoostMode.ChargeAndRelease)
                    > session.BoostVotes.Values.Count(x => x == BoostMode.Steady)
                    ? BoostMode.ChargeAndRelease : BoostMode.Steady;
                session.BoostVotePhase = VotePhase.Closed;
            }
            return RaceSnapshotFor(sessionId, session);
        }
    }

    public RaceSnapshot StartRace(string sessionId)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            if (session.BoostVotePhase == VotePhase.Open) return RaceSnapshotFor(sessionId, session);
            ResetRaceUnsafe(session);
            session.RacePhase = RacePhase.Racing;
            return RaceSnapshotFor(sessionId, session);
        }
    }

    public RaceSnapshot Boost(string sessionId, string participantId, int taps)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            if (taps <= 0 || session.RacePhase != RacePhase.Racing ||
                !session.VotesByParticipant.TryGetValue(participantId, out var champion))
            {
                return RaceSnapshotFor(sessionId, session);
            }

            var teamSize = Math.Max(1, session.VotesByParticipant.Values.Count(x => x == champion));
            var acceptedTaps = Math.Clamp(taps, 1, 20);
            var boostPerTap = 1d / (teamSize * AverageTapsPerPlayerToFinish);
            var earned = boostPerTap * acceptedTaps;
            if (session.Mode == BoostMode.ChargeAndRelease)
            {
                // Four equally sized releases; carried charge makes batch boundaries fair.
                const double burstSize = .25d;
                session.Charge[champion] += earned;
                var releases = Math.Floor((session.Charge[champion] + 1e-9d) / burstSize);
                earned = releases * burstSize;
                session.Charge[champion] = Math.Max(0, session.Charge[champion] - earned);
            }
            var progress = Math.Clamp(session.RaceProgress[champion] + earned, 0d, 1d);
            // Batched fractions can land just below 1 (for example four batches of 20 + 4 taps
            // from each of two racers). Snap numerical roundoff to the finish.
            if (progress >= 1d - 1e-9d) progress = 1d;
            session.RaceProgress[champion] = progress;

            if (progress >= 1d && session.RaceWinner is null)
            {
                session.RaceWinner = champion;
                session.RacePhase = RacePhase.Finished;
            }

            return RaceSnapshotFor(sessionId, session);
        }
    }

    public RaceSnapshot ResetRace(string sessionId)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            ResetRaceUnsafe(session);
            return RaceSnapshotFor(sessionId, session);
        }
    }

    public RaceSnapshot GetRaceSnapshot(string sessionId)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            return RaceSnapshotFor(sessionId, session);
        }
    }

    private SessionVotes GetSession(string sessionId) =>
        _sessions.GetOrAdd(Normalize(sessionId), _ => new SessionVotes());

    private static Champion? GetWinningChampion(SessionVotes session)
    {
        if (session.VotesByParticipant.Count == 0)
        {
            return null;
        }

        var counts = session.VotesByParticipant.Values
            .GroupBy(x => x)
            .Select(group => new { Champion = group.Key, Count = group.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Champion)
            .ToArray();

        if (counts.Length > 1 && counts[0].Count == counts[1].Count)
        {
            return null;
        }

        return counts[0].Champion;
    }

    private static void ResetRaceUnsafe(SessionVotes session)
    {
        session.Charge[Champion.RacingDriver] = 0;
        session.Charge[Champion.Charioteer] = 0;
        session.Charge[Champion.SpacePilot] = 0;
        session.RacePhase = RacePhase.Ready;
        session.RaceWinner = null;
        session.RaceProgress[Champion.RacingDriver] = 0d;
        session.RaceProgress[Champion.Charioteer] = 0d;
        session.RaceProgress[Champion.SpacePilot] = 0d;
    }

    private static string Normalize(string sessionId) =>
        string.IsNullOrWhiteSpace(sessionId) ? "ORBIT" : sessionId.Trim().ToUpperInvariant();

    private static VoteSnapshot Snapshot(string sessionId, SessionVotes session) =>
        new(
            Normalize(sessionId),
            session.VotesByParticipant.Values.Count(x => x == Champion.RacingDriver),
            session.VotesByParticipant.Values.Count(x => x == Champion.Charioteer),
            session.VotesByParticipant.Values.Count(x => x == Champion.SpacePilot),
            session.Phase,
            session.RevealedChampion);

    private static RaceSnapshot RaceSnapshotFor(string sessionId, SessionVotes session) =>
        new(
            Normalize(sessionId),
            session.RaceProgress[Champion.RacingDriver],
            session.RaceProgress[Champion.Charioteer],
            session.RaceProgress[Champion.SpacePilot],
            session.VotesByParticipant.Values.Count(x => x == Champion.RacingDriver),
            session.VotesByParticipant.Values.Count(x => x == Champion.Charioteer),
            session.VotesByParticipant.Values.Count(x => x == Champion.SpacePilot),
            session.RacePhase,
            session.RaceWinner,
            session.Mode,
            session.BoostVotePhase,
            session.BoostVotes.Values.Count(x => x == BoostMode.Steady),
            session.BoostVotes.Values.Count(x => x == BoostMode.ChargeAndRelease),
            session.Charge[Champion.RacingDriver] / .25d,
            session.Charge[Champion.Charioteer] / .25d,
            session.Charge[Champion.SpacePilot] / .25d);
}
