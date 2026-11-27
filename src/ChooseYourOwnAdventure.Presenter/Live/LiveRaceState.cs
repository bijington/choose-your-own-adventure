namespace ChooseYourOwnAdventure.Live;

public enum BoostMode
{
    Steady,
    ChargeAndRelease
}

public enum RacePhase
{
    Ready,
    Racing,
    Finished
}

public sealed record RaceSnapshot(
    string SessionId,
    double RacingDriverProgress,
    double CharioteerProgress,
    double SpacePilotProgress,
    int RacingDriverTeamSize,
    int CharioteerTeamSize,
    int SpacePilotTeamSize,
    RacePhase Phase,
    Champion? Winner,
    BoostMode Mode = BoostMode.Steady,
    VotePhase BoostVotePhase = VotePhase.Ready,
    int SteadyVotes = 0,
    int BurstVotes = 0,
    double RacingDriverCharge = 0,
    double CharioteerCharge = 0,
    double SpacePilotCharge = 0);

public sealed class LiveRaceState
{
    private readonly object _gate = new();
    private RaceSnapshot _snapshot = new("ORBIT", 0d, 0d, 0d, 0, 0, 0, RacePhase.Ready, null);

    public RaceSnapshot Snapshot
    {
        get
        {
            lock (_gate)
            {
                return _snapshot;
            }
        }
    }

    public void Update(RaceSnapshot snapshot)
    {
        lock (_gate)
        {
            _snapshot = snapshot;
        }
    }

    public double ProgressFor(Champion champion)
    {
        var snapshot = Snapshot;
        return champion switch
        {
            Champion.RacingDriver => snapshot.RacingDriverProgress,
            Champion.Charioteer => snapshot.CharioteerProgress,
            Champion.SpacePilot => snapshot.SpacePilotProgress,
            _ => 0d
        };
    }

    public int TeamSizeFor(Champion champion)
    {
        var snapshot = Snapshot;
        return champion switch
        {
            Champion.RacingDriver => snapshot.RacingDriverTeamSize,
            Champion.Charioteer => snapshot.CharioteerTeamSize,
            Champion.SpacePilot => snapshot.SpacePilotTeamSize,
            _ => 0
        };
    }
}
