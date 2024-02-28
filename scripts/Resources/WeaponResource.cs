using Godot;

namespace Yellow.Resources.Weapon;

[GlobalClass]
public partial class WeaponResource : Resource
{
	[Export] public string WeaponName;
	
	[Export] public string ActivateAnim;
	[Export] public string DeactivateAnim;
	[Export] public string ShootAnim;
	[Export] public string OutOfAmmoAnim;

	[Export] public int CurrentAmmo;
	[Export] public int ReserveAmmo;
	[Export] public int Magazine;
	[Export] public int MaxAmmo;

	[Export] public bool Hitscan = false;
	[Export] public bool Automatic = false;
}
