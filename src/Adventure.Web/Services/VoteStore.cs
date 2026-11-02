using Adventure.Web.Models;
using System.Collections.Concurrent;

namespace Adventure.Web.Services;

public sealed class VoteStore
{
    private sealed class SessionVotes
    {
        public object Gate { get; } = new();
        public Dictionary<string, Champion> VotesByParticipant { get; } = new();
        public VotePhase Phase { get; set; } = VotePhase.Ready;
        public Champion? RevealedChampion { get; set; }
    }

    private readonly ConcurrentDictionary<string, SessionVotes> _sessions =
        new(StringComparer.OrdinalIgnoreCase);

    public VoteSnapshot CastVote(string sessionId, string participantId, Champion champion)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            if (session.Phase == VotePhase.Open)
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
            session.Phase = VotePhase.Open;
            session.RevealedChampion = null;
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
            session.VotesByParticipant.Clear();
            session.Phase = VotePhase.Ready;
            session.RevealedChampion = null;
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

        // A tie deliberately produces no reveal. The presenter can reopen voting
        // or reset, which makes a tie a fun live moment rather than a hidden rule.
        if (counts.Length > 1 && counts[0].Count == counts[1].Count)
        {
            return null;
        }

        return counts[0].Champion;
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
}
