namespace ChooseYourOwnAdventure.Live;

public enum VotePhase
{
    Ready,
    Open,
    Closed,
    Revealed
}

public sealed record VoteSnapshot(
    string SessionId,
    int RacingDriver,
    int Charioteer,
    int SpacePilot,
    VotePhase Phase,
    Champion? RevealedChampion)
{
    public int Total => RacingDriver + Charioteer + SpacePilot;
    public bool IsOpen => Phase == VotePhase.Open;
}

public sealed class LiveVoteState
{
    private readonly object _gate = new();
    private VoteSnapshot _snapshot = new("ORBIT", 0, 0, 0, VotePhase.Ready, null);
    private DateTimeOffset _phaseChangedAt = DateTimeOffset.UtcNow;

    public VoteSnapshot Snapshot
    {
        get
        {
            lock (_gate)
            {
                return _snapshot;
            }
        }
    }

    public TimeSpan PhaseElapsed
    {
        get
        {
            lock (_gate)
            {
                return DateTimeOffset.UtcNow - _phaseChangedAt;
            }
        }
    }

    public void Update(VoteSnapshot snapshot)
    {
        lock (_gate)
        {
            if (snapshot.Phase != _snapshot.Phase)
            {
                _phaseChangedAt = DateTimeOffset.UtcNow;
            }

            _snapshot = snapshot;
        }
    }

    public Champion? WinningChampion
    {
        get
        {
            var snapshot = Snapshot;
            if (snapshot.RevealedChampion is not null)
            {
                return snapshot.RevealedChampion;
            }

            if (snapshot.Total == 0)
            {
                return null;
            }

            var max = Math.Max(snapshot.RacingDriver, Math.Max(snapshot.Charioteer, snapshot.SpacePilot));
            var winners = 0;
            Champion? winner = null;

            if (snapshot.RacingDriver == max)
            {
                winners++;
                winner = Champion.RacingDriver;
            }

            if (snapshot.Charioteer == max)
            {
                winners++;
                winner = Champion.Charioteer;
            }

            if (snapshot.SpacePilot == max)
            {
                winners++;
                winner = Champion.SpacePilot;
            }

            return winners == 1 ? winner : null;
        }
    }
}
