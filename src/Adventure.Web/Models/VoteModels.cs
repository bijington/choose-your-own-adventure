namespace Adventure.Web.Models;

public enum Champion
{
    RacingDriver,
    Charioteer,
    SpacePilot
}

public enum VotePhase
{
    Ready,
    Open,
    Closed,
    Revealed
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
    Champion? Winner);
