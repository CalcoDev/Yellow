using Godot;

namespace Yellow.Resources;

[GlobalClass]
public partial class WeaponResource : Resource
{
	[Export] public string WeaponName;
	
	[Export] public string ActivateAnim;
	[Export] public string DeactivateAnim;
	[Export] public string ShootAnim;
	[Export] public string OutOfAmmoAnim;
	[Export] public string ReloadAnim;

	[Export] public float ActivateAnimLength;
	[Export] public float DeactivateAnimLength;
	[Export] public float ShootAnimLength;
	[Export] public float OutOfAmmoAnimLength;
	[Export] public float ReloadAnimLength;

	[Export] public int CurrentAmmo;
	[Export] public int ReserveAmmo;
	[Export] public int Magazine;
	[Export] public int MaxAmmo;
	[Export] public int LowAmmoThreshold;

	[Export] public bool Hitscan = false;
	[Export] public bool Automatic = false;
}
