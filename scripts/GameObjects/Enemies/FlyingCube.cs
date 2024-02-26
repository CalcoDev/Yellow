using Godot;
using Yellow.Components;

namespace Yellow.GameObjects.Enemies;

public partial class FlyingCube : RigidBody3D
{
    [ExportGroup("References")]
    [Export] private HealthComponent _health;
    [Export] private PathfindingComponent _nav;
    [Export] private HurtboxComponent _hurtbox;

    [ExportGroup("Settings")]
    [Export] private float MoveSpeed = 12f;

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        _nav.TargetPosition = Player.Instance.GlobalPosition;
        _nav.FollowPath();
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        state.LinearVelocity = _nav.CachedDir * MoveSpeed;
    }
}