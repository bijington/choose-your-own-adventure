using Adventure.Contracts;

namespace ChooseYourOwnAdventure.Live;

public sealed class LiveSteeringState
{
    private readonly object _gate = new();
    private readonly HashSet<string> _endedRounds = new();
    private SteeringSnapshot _snapshot = new("", "", SteeringPhase.Inactive, [], null);
    public SteeringSnapshot Snapshot => Volatile.Read(ref _snapshot);
    public void Update(SteeringSnapshot snapshot)
    {
        lock (_gate)
        {
            if (snapshot.Phase == SteeringPhase.Inactive)
            {
                if (!string.IsNullOrEmpty(_snapshot.RoundId)) _endedRounds.Add(_snapshot.RoundId);
                if (!string.IsNullOrEmpty(snapshot.RoundId)) _endedRounds.Add(snapshot.RoundId);
            }
            else if (_endedRounds.Contains(snapshot.RoundId)) return;
            Volatile.Write(ref _snapshot, snapshot);
        }
    }
}
