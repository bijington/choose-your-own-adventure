namespace Adventure.Contracts;

public enum SteeringPhase { Inactive, Ready, Racing, Finished }
public enum PowerSide { None, Left, Right, Both }
public sealed record SteeringVehicle(int Champion, double Progress, double Offset, double TargetOffset,
    double LeftPower, double RightPower, int LeftPlayers, int RightPlayers);
public sealed record SteeringSnapshot(string SessionId, string RoundId, SteeringPhase Phase,
    SteeringVehicle[] Vehicles, int? Winner);
