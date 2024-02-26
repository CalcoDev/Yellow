using Godot;
using System;
using Yellow.Resources.Items;

public partial class Gun : Node
{
	[Export] public GunResource Data;
	[Export] public RayCast3D EntityLookingAt;
	[Export] public MeshInstance3D Model;
	[Export] public Bullet BulletType; 
	
	public override void _Ready() { }

	public override void _Process(double delta)
	{
		if(Data.CooldownTimer > 0) Data.CooldownTimer -= delta;
	}

	public virtual void Shoot(Vector3 direction)
	{
		if (Data.BulletCount <= 0) { OnShootOutOfAmmo(); return; }
		if (Data.CooldownTimer > 0) { return; }

		var bulletsToExpend = Mathf.Min(Data.BulletsPerShot, Data.BulletCount);
		
		// Allow at least one perfectly accurate shot.
		CreateShot(direction, 0.0f);
		for (var i = 2; i <= bulletsToExpend; i++) CreateShot(direction, Data.MaxSpreadAngle);
		
		GD.Print($"Bullets remaining: {Data.BulletCount}");
	}
	
	public virtual void Reload() { }
	
	public virtual void Scope() { }
	
	public virtual void OnShootOutOfAmmo() { }

	public virtual void CreateShot(Vector3 direction, float maxDeviation)
	{
		Data.BulletCount--;
		direction = MakeRandomSpread(direction, maxDeviation);
		if(Data.Hitscan) DoHitscanShot(direction);
		else DoBulletShot(direction); 
	}

	public virtual void DoHitscanShot(Vector3 direction)
	{
		EntityLookingAt.ForceRaycastUpdate();

		GodotObject theCollider = EntityLookingAt.GetCollider();
		if (theCollider is not CollisionShape3D)
		{
			GD.Print("Missed shot.");
			return;
		}

		Vector3 collisionPoint = EntityLookingAt.GetCollisionPoint();
		GD.Print($"Hit wall at {collisionPoint.X} {collisionPoint.Y} {collisionPoint.Z}.");
	}

	public virtual void DoBulletShot(Vector3 direction)
	{
		var bulletToShoot = 
	}

	public virtual Vector3 MakeRandomSpread(Vector3 direction, float maxDeviation)
	{
		return new Vector3(
			direction.X + (float)(GD.RandRange(-maxDeviation, maxDeviation)),
			direction.Y + (float)(GD.RandRange(-maxDeviation, maxDeviation)),
			direction.Z
		);
	}
}
