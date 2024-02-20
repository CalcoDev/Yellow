using Godot;

namespace Yellow.Managers;

public partial class Game : Node
{
    public static Game Instance { get; private set; }
    
    // TIME
    public static float DeltaTime { get; private set; }
    public static float FixedDeltaTime { get; private set; }
    public static float Time { get; private set; }
    public static float FixedTime { get; private set; }

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            GD.PushWarning("WARN: Game Manager already exists!");
            QueueFree();
            return;
        }

        Instance = this;
    }

    public override void _Process(double delta)
    {
        DeltaTime = (float) delta;
        Time += DeltaTime;
    }

    public override void _PhysicsProcess(double delta)
    {
        FixedDeltaTime = (float) delta;
        FixedTime += FixedDeltaTime;
    }
}