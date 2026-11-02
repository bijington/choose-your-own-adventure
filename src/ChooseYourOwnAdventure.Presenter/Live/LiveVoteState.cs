namespace ChooseYourOwnAdventure.Live;

public sealed record VoteSnapshot(
    string SessionId,
    int RacingDriver,
    int Charioteer,
    int SpacePilot,
    bool IsOpen)
{
    public int Total => RacingDriver + Charioteer + SpacePilot;
}

public sealed class LiveVoteState
{
    private readonly object _gate = new();
    private VoteSnapshot _snapshot = new("ORBIT", 0, 0, 0, true);

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

    public void Update(VoteSnapshot snapshot)
    {
        lock (_gate)
        {
            _snapshot = snapshot;
        }
    }

    public Champion? WinningChampion
    {
        get
        {
            var snapshot = Snapshot;
            if (snapshot.Total == 0)
            {
                return null;
            }

            var max = Math.Max(snapshot.RacingDriver, Math.Max(snapshot.Charioteer, snapshot.SpacePilot));
            if (snapshot.RacingDriver == max)
            {
                return Champion.RacingDriver;
            }

            if (snapshot.Charioteer == max)
            {
                return Champion.Charioteer;
            }

            return Champion.SpacePilot;
        }
    }
}
