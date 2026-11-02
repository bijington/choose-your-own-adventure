using Microsoft.AspNetCore.SignalR.Client;

namespace ChooseYourOwnAdventure.Live;

public sealed class AudienceVoteClient : IAsyncDisposable
{
    private readonly LiveAudienceOptions _options;
    private readonly LiveVoteState _voteState;
    private HubConnection? _connection;
    private int _started;

    public AudienceVoteClient(LiveAudienceOptions options, LiveVoteState voteState)
    {
        _options = options;
        _voteState = voteState;
    }

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
        _connection.Reconnected += async _ => await JoinSessionAsync();

        // Never block the presentation while a network connection is established.
        _ = ConnectUntilAvailableAsync();
        return Task.CompletedTask;
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
