using Godot;
using Yellow.Components;

namespace Yellow.GameObjects.Enemies;

public partial class Enemy : RigidBody3D
{
    [ExportGroup("Enemy Settings")]
    [ExportSubgroup("References")]
    [Export] protected HealthComponent Health;
    [Export] protected FactionComponent Faction;
    [Export] protected KnockBackComponent KnockBack;
    [Export] protected PathfindingComponent Pathfinding;
    [Export] protected HurtboxComponent HurtBox;
    [Export] protected AnimationPlayer Anim;

    [ExportSubgroup("General")]
    public bool ShouldDoStuff { get; set; }= true;

	public override void _Ready()
	{
		HurtBox.OnHit += OnHit;
		Health.OnDied += OnDied;
	}
	
	public override void _Process(double delta)
	{
		
	}

	protected virtual void OnHit(HitboxComponent hitBox)
	{
		
	}

	protected virtual void OnDied()
	{
		QueueFree();
	}
}
