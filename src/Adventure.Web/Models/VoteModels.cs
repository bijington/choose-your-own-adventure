namespace Adventure.Web.Models;

public enum Champion
{
    RacingDriver,
    Charioteer,
    SpacePilot
}

public sealed record VoteSnapshot(
    string SessionId,
    int RacingDriver,
    int Charioteer,
    int SpacePilot,
    bool IsOpen)
{
    public int Total => RacingDriver + Charioteer + SpacePilot;
}
