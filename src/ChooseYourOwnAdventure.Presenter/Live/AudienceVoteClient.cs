using Microsoft.AspNetCore.SignalR.Client;

namespace ChooseYourOwnAdventure.Live;

public sealed class AudienceVoteClient : IAsyncDisposable
{
    private readonly LiveAudienceOptions _options;
    private readonly LiveVoteState _voteState;
    private readonly LiveRaceState _raceState;
    private HubConnection? _connection;
    private int _started;

    public AudienceVoteClient(LiveAudienceOptions options, LiveVoteState voteState, LiveRaceState raceState)
    {
        _options = options;
        _voteState = voteState;
        _raceState = raceState;
    }

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public Task StartAsync()
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
        {
            return Task.CompletedTask;
        }

        _connection = new HubConnectionBuilder()
            .WithUrl(_options.HubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<VoteSnapshot>("VoteUpdated", _voteState.Update);
        _connection.On<RaceSnapshot>("RaceUpdated", _raceState.Update);
        _connection.Reconnected += async _ => await JoinSessionAsync();

        // Never block the presentation while a network connection is established.
        _ = ConnectUntilAvailableAsync();
        return Task.CompletedTask;
    }

    public Task OpenVotingAsync() => InvokePresenterCommandAsync("OpenVoting");
    public Task CloseVotingAsync() => InvokePresenterCommandAsync("CloseVoting");
    public Task RevealWinnerAsync() => InvokePresenterCommandAsync("RevealWinner");
    public Task ResetVotingAsync() => InvokePresenterCommandAsync("ResetVoting");
    public Task StartRaceAsync() => InvokePresenterCommandAsync("StartRace");
    public Task ResetRaceAsync() => InvokePresenterCommandAsync("ResetRace");

    private async Task InvokePresenterCommandAsync(string methodName)
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            return;
        }

        try
        {
            await _connection.InvokeAsync(methodName, _options.SessionId, _options.PresenterKey);
        }
        catch
        {
            // Presenter controls should never crash the talk because the network dropped.
        }
    }

    private async Task ConnectUntilAvailableAsync()
    {
        while (_connection is { State: HubConnectionState.Disconnected })
        {
            try
            {
                await _connection.StartAsync();
                await JoinSessionAsync();
                return;
            }
            catch
            {
                // Conference networking can be unpredictable. Keep the presentation
                // usable offline and quietly retry the audience connection.
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }

    private async Task JoinSessionAsync()
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            return;
        }

        await _connection.InvokeAsync("JoinSession", _options.SessionId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
