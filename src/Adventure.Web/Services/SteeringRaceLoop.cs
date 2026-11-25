using Adventure.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Adventure.Web.Services;

public sealed class SteeringRaceLoop(SteeringRaceStore store, IHubContext<AdventureHub> hub, ILogger<SteeringRaceLoop> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var session in store.ActiveSessions)
            {
                var snapshot = store.Step(session, .05);
                if (snapshot is not null)
                {
                    try { await hub.Clients.Group(session).SendAsync("SteeringUpdated", snapshot, stoppingToken); }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
                    catch (Exception error) { logger.LogWarning(error, "Bonus-race broadcast failed; continuing the loop."); }
                }
            }
        }
    }
}
