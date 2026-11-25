using Microsoft.AspNetCore.SignalR.Client;
using Adventure.Contracts;

namespace ChooseYourOwnAdventure.Live;

public sealed class AudienceVoteClient : IAsyncDisposable
{
    private readonly LiveAudienceOptions _options;
    private readonly AdventureState _state;
    private readonly LiveVoteState _voteState;
    private readonly LiveRaceState _raceState;
    public LiveSteeringState Steering { get; } = new();
    private HubConnection? _connection;
    private int _started;

    public AudienceVoteClient(LiveAudienceOptions options, LiveVoteState voteState, LiveRaceState raceState, AdventureState state)
    {
        _options = options;
        _state = state;
        _voteState = voteState;
        _raceState = raceState;
    }

    public bool IsConnected => !_state.IsRehearsing && _connection?.State == HubConnectionState.Connected;

    public Task StartAsync()
    {
        if (_state.IsRehearsing) return Task.CompletedTask;
        if (Interlocked.Exchange(ref _started, 1) == 1)
        {
            return IsConnected ? JoinSessionAsync() : Task.CompletedTask;
        }

        _connection = new HubConnectionBuilder()
            .WithUrl(_options.HubUrl)
            .WithAutomaticReconnect()
            .Build();

        var connection = _connection;
        _connection.On<VoteSnapshot>("VoteUpdated", snapshot =>
        {
            if (!_state.IsRehearsing && ReferenceEquals(_connection, connection) && string.Equals(snapshot.SessionId, _options.SessionId, StringComparison.OrdinalIgnoreCase)) _voteState.Update(snapshot);
        });
        _connection.On<RaceSnapshot>("RaceUpdated", snapshot =>
        {
            if (!_state.IsRehearsing && ReferenceEquals(_connection, connection) && string.Equals(snapshot.SessionId, _options.SessionId, StringComparison.OrdinalIgnoreCase)) _raceState.Update(snapshot);
        });
        _connection.On<SteeringSnapshot>("SteeringUpdated", snapshot =>
        {
            if (!_state.IsRehearsing && ReferenceEquals(_connection, connection)
                && string.Equals(snapshot.SessionId, _options.SessionId, StringComparison.OrdinalIgnoreCase))
                Steering.Update(snapshot);
        });
        _connection.Reconnected += async _ => await JoinSessionAsync();

        // Never block the presentation while a network connection is established.
        _ = ConnectUntilAvailableAsync(_connection);
        return Task.CompletedTask;
    }

    public async Task<string> CreateSessionAsync()
    {
        if (IsConnected) await StopSteeringRaceAsync();
        await PauseAsync();
        var session = _options.CreateSession();
        Steering.Update(new SteeringSnapshot(session, "", SteeringPhase.Inactive, [], null));
        _state.ResetLocalVote();
        _voteState.Update(new VoteSnapshot(session, 0, 0, 0, VotePhase.Ready, null));
        _raceState.Update(new RaceSnapshot(session, 0, 0, 0, 0, 0, 0, RacePhase.Ready, null));
        await StartAsync();
        return session;
    }

    public Task PrepareSteeringRaceAsync() => InvokePresenterCommandAsync("PrepareSteeringRace");
    public Task StartSteeringRaceAsync() => InvokePresenterCommandAsync("StartSteeringRace");
    public async Task StopSteeringRaceAsync()
    {
        await InvokePresenterCommandAsync("StopSteeringRace");
        var snapshot = Steering.Snapshot;
        Steering.Update(snapshot with { Phase = SteeringPhase.Inactive });
    }

    public Task OpenVotingAsync() => InvokePresenterCommandAsync("OpenVoting");
    public Task CloseVotingAsync() => InvokePresenterCommandAsync("CloseVoting");
    public Task RevealWinnerAsync() => InvokePresenterCommandAsync("RevealWinner");
    public Task ResetVotingAsync() => InvokePresenterCommandAsync("ResetVoting");
    public Task OpenBoostVotingAsync() => InvokePresenterCommandAsync("OpenBoostVoting");
    public Task CloseBoostVotingAsync() => InvokePresenterCommandAsync("CloseBoostVoting");
    public Task StartRaceAsync() => InvokePresenterCommandAsync("StartRace");
    public Task ResetRaceAsync() => InvokePresenterCommandAsync("ResetRace");

    private async Task InvokePresenterCommandAsync(string methodName)
    {
        if (_state.IsRehearsing || _connection?.State != HubConnectionState.Connected)
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

    public async Task PauseAsync()
    {
        var connection = _connection;
        _connection = null;
        Interlocked.Exchange(ref _started, 0);
        if (connection is not null) await connection.DisposeAsync();
    }

    private async Task ConnectUntilAvailableAsync(HubConnection connection)
    {
        while (ReferenceEquals(_connection, connection) && !_state.IsRehearsing
            && connection.State == HubConnectionState.Disconnected)
        {
            try
            {
                await connection.StartAsync();
                if (ReferenceEquals(_connection, connection)) await JoinSessionAsync();
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
        if (_state.IsRehearsing || _connection?.State != HubConnectionState.Connected)
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
