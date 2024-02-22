namespace Yellow.Misc;

public enum NodeProcessOrder
{
    Game = -999,
    PlayerInput = -900,
    GroundChecks = -800,
    ShapeChecks = -700,
    Player = -500,
    Default = 0,
    InterpolationComponents = 100,
}