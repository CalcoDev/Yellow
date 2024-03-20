using Godot;
using Yellow.Components;
using Yellow.Extensions;
using Yellow.Managers;
using Yellow.Misc;

namespace Yellow.GameObjects.Projectiles;

public partial class Arrow : RigidBody3D
{
	public Vector3 Trajectory;
	public float Damage;
	public int ChargePower;
	
	private HitboxComponent _hitbox;
	private bool _exhausted = false;
	
	public override void _Ready()
	{
		_hitbox = GetNode("HitboxComponent") as HitboxComponent;
		if(_hitbox == null) return;
		_hitbox.AreaEntered += OnAreaEntered;
	}
	
	public override void _Process(double delta)
	{
		_hitbox.Damage = Damage;
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
		// if(_exhausted) return;
		
		// var hurtbox = (body is HurtboxComponent component ? component : body.GetFirstNodeOfType<HurtboxComponent>());
		// if(hurtbox == null) return;

		// GD.Print(ChargePower);
		// if(ChargePower == 10)
		// 	SoundManager.Instance.Play("HitFullPower");
		
		// _hitbox.Damage = Damage;
		// hurtbox.TryToHit(_hitbox);
		// hurtbox.KnockBackComponent?.ApplyKnockBack(
		// 	new KnockBackData(
		// 		Player.Instance, 
		// 		new Vector3(1.0f + 0.1f * ChargePower, 4.0f, 1.0f + 0.1f * ChargePower),
		// 		3.0f
		// 	)
		// );
		
		// _exhausted = true;
		QueueFree();
	}
}
