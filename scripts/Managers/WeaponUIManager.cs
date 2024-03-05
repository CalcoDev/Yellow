using Godot;
using System;
using Yellow.Misc;
using Yellow.Resources;

namespace Yellow.Managers;

public partial class WeaponUIManager : Node
{
	private Label _currentAmmoLabel, _reserveAmmoLabel;
	private Label _nameLabel;
	
	public override void _Ready()
	{
		_currentAmmoLabel = GetNode("MainContainer/WeaponAmmo/CurrentAmmo") as Label;
		_reserveAmmoLabel = GetNode("MainContainer/WeaponAmmo/ReserveAmmo") as Label;
		_nameLabel = GetNode("MainContainer/WeaponName/Name") as Label;	
	}
	
	public override void _Process(double delta)
	{
		
	}

	public void UpdateAllWeaponInfo(WeaponResource currentWeapon)
	{
		UpdateAmmoLabel(currentWeapon);
		UpdateNameLabel(currentWeapon);
	}
	
	public void UpdateAmmoLabel(WeaponResource currentWeapon)
	{
		_currentAmmoLabel.Text = $" {currentWeapon.CurrentAmmo}";

		_currentAmmoLabel.LabelSettings.FontColor = 
			currentWeapon.CurrentAmmo <= currentWeapon.LowAmmoThreshold ? Colors.Red : Colors.White;
		
		_reserveAmmoLabel.Text = $" / {currentWeapon.ReserveAmmo}";
	}

	public void UpdateNameLabel(WeaponResource currentWeapon)
	{
		_nameLabel.Text = $"  Holding: {currentWeapon.WeaponName}";
	}
}
