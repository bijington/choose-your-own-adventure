namespace Adventure.Web.Models;

public enum Champion
{
    RacingDriver = 20,
    Charioteer = 50,
    SpacePilot = 80
}

public enum VotePhase
{
    Ready,
    Open,
    Closed,
    Revealed
}

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
