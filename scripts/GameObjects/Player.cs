using Godot;
using Yellow.Components;
using Yellow.Managers;
using Yellow.Misc;
using Yellow.Resources;

namespace Yellow.GameObjects;

public partial class Player : RigidBody3D
{
    [ExportGroup("References")]
    [Export] private PlayerInput _input;
    [Export] private GroundCheck _groundCheck;

    [ExportGroup("Movement")]
    [ExportSubgroup("Run")]
    [Export] private float RunSpeed;
    [Export] private float RunAccel;
    [Export] private float AirAccelMult;
    [Export] private float AirDecelMult;

    [ExportSubgroup("Jump")]
    [Export] private float JumpForce;
    [Export] private float JumpReleaseMult;

    // States

    int ic = 0;
    int pc = 0;
    int fc = 0;
    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        // GD.Print("INTEGRATE ", ic, " | TIME: ", Time.GetTicksUsec());
        ic++;
    }

    public override void _Ready()
    {
        ProcessPriority = (int)NodeProcessOrder.Player;
    }

    public override void _Process(double delta)
    {
        // GD.Print("UPDATE ", pc, " | TIME: ", Time.GetTicksUsec());
        pc++;
    }

    public override void _PhysicsProcess(double delta)
    {
        // GD.Print("PHYISCS ", fc, " | TIME: ", Time.GetTicksUsec());
        fc++;
    }
}