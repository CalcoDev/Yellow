using Godot;
using System;
using Yellow.Components;

namespace Yellow.GameObjects.Enemies;

[GlobalClass]
public partial class Enemy : RigidBody3D
{
	[ExportGroup("Components")] 
	[Export] protected HealthComponent Health;
	[Export] protected FactionComponent Faction;
	[Export] protected KnockBackComponent KnockBack;
	[Export] protected HurtboxComponent HurtBox;
	[Export] protected PathfindingComponent Pathfinding;
    
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
		GD.Print("The Enemy was hit for " + hitBox.Damage + " Damage.");
	}

	protected virtual void OnDied()
	{
		QueueFree();
	}
}
