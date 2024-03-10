using Godot;
using System;
using Yellow.Components;
using Yellow.GameObjects.Weapons;
using Yellow.Misc;
using Yellow.Resources;

[GlobalClass]
public partial class Gun : Weapon
{
	[Export] public WeaponResource WeaponResource;
	[Export] public RayCast3D HitscanRay;
	public AnimationPlayer Anim;
	
	public override void _Ready()
	{
		HitscanRay ??= GetNode("Hitscan") as RayCast3D;
		Anim = GetNode("Animation") as AnimationPlayer;
	}

	public override void _Process(double delta)
	{
		
	}

	public override double HandleInput(string inputName)
	{
		return inputName switch
		{
			"use_primary" => Shoot(),
			"use_secondary" => Scope(),
			"reload" => Reload(),
			_ => 0.0
		};
	}

	public override double Equip()
	{
		base.Equip();
		Anim.Queue(WeaponResource.ActivateAnim);
		
		return WeaponResource.ActivateAnimLength;
	}

	public override double Unequip()
	{
		Anim.Queue(WeaponResource.DeactivateAnim);
		base.Unequip();
		
		return WeaponResource.DeactivateAnimLength;
	}

	public double Shoot()
	{
		if (WeaponResource.CurrentAmmo <= 0)
			return OutOfAmmo();

		WeaponResource.CurrentAmmo--;
		
		HitscanRay.ForceRaycastUpdate();
		
		if (HitscanRay.GetCollider() is HurtboxComponent hc)
		{
			hc.HealthComponent.TakeDamage(WeaponResource.Damage);
			GD.Print("Damaged " + hc.Name + " for " + WeaponResource.Damage);
		}

		//EmitSignal(SignalName.AmmoChanged, WeaponResource);
		Anim.Queue(WeaponResource.ShootAnim);
		return WeaponResource.ShootAnimLength;
	}

	public double Scope()
	{
		return 0.0;
	}

	public double Reload()
	{
		var ammoToLoad = Math.Min(WeaponResource.ReserveAmmo, WeaponResource.Magazine - WeaponResource.CurrentAmmo);
		if(ammoToLoad == 0) return 0.0;
		
		WeaponResource.ReserveAmmo -= ammoToLoad;
		WeaponResource.CurrentAmmo += ammoToLoad;
		
		//EmitSignal(SignalName.AmmoChanged, WeaponResource);
		Anim.Queue(WeaponResource.ReloadAnim);
		return WeaponResource.ReloadAnimLength;
	}

	private double OutOfAmmo()
	{
		Anim.Queue(WeaponResource.OutOfAmmoAnim);
		return WeaponResource.OutOfAmmoAnimLength;
	}
}
