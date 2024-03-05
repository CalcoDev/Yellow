using Godot;
using System;

namespace Yellow.Resources.Items;

public partial class Gun : Item
{
    [Export] public float Damage;

    [Export] public int MaxAmmo;
    public int CurrentAmmo;

    [Export] public float ShotCooldown;
    public float CurrentShotCooldown;
	
    [Export] public float ReloadCooldown;
    public float CurrentReloadCooldown;

    [Export] public bool Hitscan;

    public void Shoot()
    {
        
    }

    public void Scope()
    {
        
    }

    public void Reload()
    {
        
    }
}