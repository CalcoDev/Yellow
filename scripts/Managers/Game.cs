using Godot;
using Yellow.Misc;

namespace Yellow.Managers;

public partial class Game : Node
{
    public static Game Instance { get; private set; }
    
    // TIME
    public static float DeltaTime { get; private set; }
    public static float FixedDeltaTime { get; private set; }
    public static float Time { get; private set; }
    public static float FixedTime { get; private set; }

    // MOUSE
    public static bool MouseLocked
    {
        get => _mouseLocked;
        set {
            _mouseLocked = value;
            Input.MouseMode = value ? 
                Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        }
    }
    private static bool _mouseLocked;

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            GD.PushWarning("WARN: Game Manager already exists!");
            QueueFree();
            return;
        }

        Instance = this;

        // Make this node execute first, always.
        ProcessPriority = (int)NodeProcessOrder.Game;
    }

    public override void _Ready()
    {
        MouseLocked = true;
    }

    public override void _Process(double delta)
    {
        DeltaTime = (float) delta;
        Time += DeltaTime;

        if (Input.IsActionJustPressed("quit")) {
            GetTree().Quit();
        }

        if (Input.IsActionJustPressed("unfocus")) {
            MouseLocked = !MouseLocked;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        FixedDeltaTime = (float) delta;
        FixedTime += FixedDeltaTime;
    }
}