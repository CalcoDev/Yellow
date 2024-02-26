using Godot;

namespace Yellow.Resources.Items;

[GlobalClass]
public partial class GunResource : ItemResource
{
	[Export] public float Damage = 1.0f;
	
	[Export] public int MagazineCapacity = 10;
	[Export] public int BulletCount = 10;
	[Export] public int BulletsPerShot = 1;

	[Export] public float MaxSpreadAngle = 10.0f;
	
	[Export] public bool Hitscan = false;
	[Export] public float BulletSpeed = 200.0f;

	[Export] public double ShootCooldown = 1.0f;
	[Export] public double ReloadCooldown = 2.5f;
	[Export] public double CooldownTimer = 1.0f;
}