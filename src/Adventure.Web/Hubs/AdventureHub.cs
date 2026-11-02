using Adventure.Web.Models;
using Adventure.Web.Services;
using Microsoft.AspNetCore.SignalR;

namespace Adventure.Web.Hubs;

public sealed class AdventureHub : Hub
{
    private readonly VoteStore _voteStore;
    private readonly string _presenterKey;

    public AdventureHub(VoteStore voteStore, IConfiguration configuration)
    {
        _voteStore = voteStore;
        _presenterKey = configuration["Presenter:Key"] ?? "orbit-local-presenter";
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

    public Task OpenVoting(string sessionId, string presenterKey) =>
        PresenterBroadcast(sessionId, presenterKey, _voteStore.OpenVoting);

    public Task CloseVoting(string sessionId, string presenterKey) =>
        PresenterBroadcast(sessionId, presenterKey, _voteStore.CloseVoting);

    public Task RevealWinner(string sessionId, string presenterKey) =>
        PresenterBroadcast(sessionId, presenterKey, _voteStore.RevealWinner);

    public Task ResetVoting(string sessionId, string presenterKey) =>
        PresenterBroadcast(sessionId, presenterKey, _voteStore.Reset);

    private async Task PresenterBroadcast(
        string sessionId,
        string presenterKey,
        Func<string, VoteSnapshot> action)
    {
        if (!string.Equals(presenterKey, _presenterKey, StringComparison.Ordinal))
        {
            throw new HubException("Presenter control denied.");
        }

        var normalized = Normalize(sessionId);
        var snapshot = action(normalized);
        await Clients.Group(normalized).SendAsync("VoteUpdated", snapshot);
    }

    private static string Normalize(string sessionId) =>
        string.IsNullOrWhiteSpace(sessionId) ? "ORBIT" : sessionId.Trim().ToUpperInvariant();
}
