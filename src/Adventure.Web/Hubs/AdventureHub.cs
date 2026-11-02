using Adventure.Web.Models;
using Adventure.Web.Services;
using Microsoft.AspNetCore.SignalR;

namespace Adventure.Web.Hubs;

public sealed class AdventureHub : Hub
{
    private readonly VoteStore _voteStore;

    public AdventureHub(VoteStore voteStore)
    {
        _voteStore = voteStore;
    }

    public async Task JoinSession(string sessionId)
    {
        var normalized = Normalize(sessionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, normalized);
        await Clients.Caller.SendAsync("VoteUpdated", _voteStore.GetSnapshot(normalized));
    }

    public async Task Vote(string sessionId, string participantId, string championName)
    {
        var normalized = Normalize(sessionId);
        if (!Enum.TryParse<Champion>(championName, ignoreCase: true, out var champion))
        {
            throw new HubException("Unknown champion.");
        }

        var participant = string.IsNullOrWhiteSpace(participantId) ? Context.ConnectionId : participantId.Trim();
        var snapshot = _voteStore.CastVote(normalized, participant, champion);
        await Clients.Group(normalized).SendAsync("VoteUpdated", snapshot);
    }


    private static string Normalize(string sessionId) =>
        string.IsNullOrWhiteSpace(sessionId) ? "ORBIT" : sessionId.Trim().ToUpperInvariant();
}
