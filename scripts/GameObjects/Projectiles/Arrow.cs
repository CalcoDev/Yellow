using Godot;
using Yellow.Components;
using Yellow.Extensions;
using Yellow.Misc;

namespace Yellow.GameObjects.Projectiles;

public partial class Arrow : RigidBody3D
{
	public Vector3 Trajectory;
	public float Damage;
	private HitboxComponent _hitbox;
	
	public override void _Ready()
	{
		_hitbox = GetNode("HitboxComponent") as HitboxComponent;
		if(_hitbox == null) return;
		_hitbox.AreaEntered += OnAreaEntered;
	}
	
	public override void _Process(double delta)
	{
		if(Trajectory.LengthSquared() <= 0.01f)
			QueueFree();
	}

	public override void _PhysicsProcess(double delta)
	{
		ApplyForce(Trajectory);
		Trajectory *= 0.8f;
	}

	private void OnAreaEntered(Node body)
	{
		var hurtbox = (body is HurtboxComponent component ? component : body.GetFirstNodeOfType<HurtboxComponent>());
		if(hurtbox == null) return;

		_hitbox.Damage = Damage;
		hurtbox.TryToHit(_hitbox);
		QueueFree();
	}
}
