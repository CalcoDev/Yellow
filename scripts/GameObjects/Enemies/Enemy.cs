using Godot;
using Yellow.Components;

namespace Yellow.GameObjects.Enemies;

public partial class Enemy : RigidBody3D
{
    [ExportGroup("Enemy Settings")]
    [ExportSubgroup("References")]
    [Export] protected HealthComponent _health;
    [Export] protected PathfindingComponent _nav;
    [Export] protected HurtboxComponent _hurtbox;
    [Export] protected AnimationPlayer _anim;

    [ExportSubgroup("General")]
    public bool ShouldDoStuff { get; set; }= true;
}