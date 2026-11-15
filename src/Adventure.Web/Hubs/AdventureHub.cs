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
        await Clients.Caller.SendAsync("RaceUpdated", _voteStore.GetRaceSnapshot(normalized));
    }

    public async Task Vote(string sessionId, string participantId, string championName)
    {
        var normalized = Normalize(sessionId);
        if (!Enum.TryParse<Champion>(championName, ignoreCase: true, out var champion) || !Enum.IsDefined(champion))
        {
            throw new HubException("Unknown champion.");
        }

        var participant = NormalizeParticipant(participantId);
        var snapshot = _voteStore.CastVote(normalized, participant, champion);
        await Clients.Group(normalized).SendAsync("VoteUpdated", snapshot);
    }

    public async Task Boost(string sessionId, string participantId, int taps)
    {
        var normalized = Normalize(sessionId);
        var participant = NormalizeParticipant(participantId);
        var snapshot = _voteStore.Boost(normalized, participant, taps);
        await Clients.Group(normalized).SendAsync("RaceUpdated", snapshot);
    }

    public Task OpenVoting(string sessionId, string presenterKey) =>
        PresenterVoteBroadcast(sessionId, presenterKey, _voteStore.OpenVoting);

    public Task CloseVoting(string sessionId, string presenterKey) =>
        PresenterVoteBroadcast(sessionId, presenterKey, _voteStore.CloseVoting);

    public Task RevealWinner(string sessionId, string presenterKey) =>
        PresenterVoteBroadcast(sessionId, presenterKey, _voteStore.RevealWinner);

    public Task ResetVoting(string sessionId, string presenterKey) =>
        PresenterVoteBroadcast(sessionId, presenterKey, _voteStore.Reset);

    public Task StartRace(string sessionId, string presenterKey) =>
        PresenterRaceBroadcast(sessionId, presenterKey, _voteStore.StartRace);

    public Task ResetRace(string sessionId, string presenterKey) =>
        PresenterRaceBroadcast(sessionId, presenterKey, _voteStore.ResetRace);

    private async Task PresenterVoteBroadcast(
        string sessionId,
        string presenterKey,
        Func<string, VoteSnapshot> action)
    {
        EnsurePresenter(presenterKey);
        var normalized = Normalize(sessionId);
        var snapshot = action(normalized);
        await Clients.Group(normalized).SendAsync("VoteUpdated", snapshot);

        // Opening/resetting voting can also reset the race state.
        await Clients.Group(normalized).SendAsync("RaceUpdated", _voteStore.GetRaceSnapshot(normalized));
    }

    private async Task PresenterRaceBroadcast(
        string sessionId,
        string presenterKey,
        Func<string, RaceSnapshot> action)
    {
        EnsurePresenter(presenterKey);
        var normalized = Normalize(sessionId);
        var snapshot = action(normalized);
        await Clients.Group(normalized).SendAsync("RaceUpdated", snapshot);
    }

    private void EnsurePresenter(string presenterKey)
    {
        if (!string.Equals(presenterKey, _presenterKey, StringComparison.Ordinal))
        {
            throw new HubException("Presenter control denied.");
        }
    }

    private string NormalizeParticipant(string participantId) =>
        string.IsNullOrWhiteSpace(participantId) ? Context.ConnectionId : participantId.Trim();

    private static string Normalize(string sessionId) =>
        string.IsNullOrWhiteSpace(sessionId) ? "ORBIT" : sessionId.Trim().ToUpperInvariant();
}
