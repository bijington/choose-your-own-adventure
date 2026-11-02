using Adventure.Web.Models;
using System.Collections.Concurrent;

namespace Adventure.Web.Services;

public sealed class VoteStore
{
    private sealed class SessionVotes
    {
        public object Gate { get; } = new();
        public Dictionary<string, Champion> VotesByParticipant { get; } = new();
        public bool IsOpen { get; set; } = true;
    }

    private readonly ConcurrentDictionary<string, SessionVotes> _sessions =
        new(StringComparer.OrdinalIgnoreCase);

    public VoteSnapshot CastVote(string sessionId, string participantId, Champion champion)
    {
        var session = GetSession(sessionId);
        lock (session.Gate)
        {
            if (session.IsOpen)
            {
                session.VotesByParticipant[participantId] = champion;
            }

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

    private static string Normalize(string sessionId) =>
        string.IsNullOrWhiteSpace(sessionId) ? "ORBIT" : sessionId.Trim().ToUpperInvariant();

    private static VoteSnapshot Snapshot(string sessionId, SessionVotes session) =>
        new(
            Normalize(sessionId),
            session.VotesByParticipant.Values.Count(x => x == Champion.RacingDriver),
            session.VotesByParticipant.Values.Count(x => x == Champion.Charioteer),
            session.VotesByParticipant.Values.Count(x => x == Champion.SpacePilot),
            session.IsOpen);
}
