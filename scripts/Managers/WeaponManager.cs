using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Yellow.Misc;
using Yellow.Resources;

namespace Yellow.Managers;

public partial class WeaponManager : Node3D
{
	[ExportGroup("Weapons")]
	[Export] public WeaponResource[] WeaponResources;
	[Export] public string[] StartWeapons;

	private AnimationPlayer _animationPlayer;

	private Dictionary<string, WeaponResource> _weaponDictionary = new();
	private List<string> _weaponList = new();
	private WeaponResource _currentWeapon;
	
	private int _weaponIndicator = 0;
	private string _nextWeapon;
	
	private double _actionCooldown = 0.0;
	private bool _holdingDownPrimary = false;

	private RayCast3D _hitscanRay;

	private PackedScene _debugBulletDecal;

	[Signal]
	public delegate void AmmoChangedEventHandler(WeaponResource currentWeapon);

	[Signal]
	public delegate void WeaponChangedEventHandler(WeaponResource currentWeapon);
	
	public override void _Ready()
	{
		_animationPlayer = GetNode("Models/AnimationPlayer") as AnimationPlayer;
		_hitscanRay = GetNode("HitscanRay") as RayCast3D;
		_debugBulletDecal = ResourceLoader.Load("res://scenes/game_objects/weapons/bullet_debug_decal.tscn") as PackedScene;
		Initialize();
	}
	public override void _Process(double delta)
	{
		_actionCooldown = Math.Max(0.0, _actionCooldown - delta);
		HandleAction();
	}

	public override void _UnhandledInput(InputEvent e)
	{
		
	}
	
	private void Initialize()
	{
		foreach (var weapon in WeaponResources)
			_weaponDictionary[weapon.WeaponName] = weapon;

		foreach (var weapon in StartWeapons)
			_weaponList.Add(weapon);

		_currentWeapon = _weaponDictionary[_weaponList[0]];
		EmitSignal(SignalName.WeaponChanged, _currentWeapon);
		
		Enter();
	}

	private void HandleAction()
	{
		if(_actionCooldown > 0.0) return;
		
		if (Input.IsActionJustPressed("weapon_up"))
		{
			_weaponIndicator++;
			_weaponIndicator %= _weaponList.Count;
			ChangeWeapon();
		}

		if (Input.IsActionJustPressed("weapon_down"))
		{
			_weaponIndicator--;
			if (_weaponIndicator < 0) _weaponIndicator = _weaponList.Count - 1; 
			ChangeWeapon();
		}

		if (Input.IsActionPressed("use_primary"))
		{
			if
			(
				!_holdingDownPrimary ||
				(_holdingDownPrimary && _currentWeapon.Automatic)
			) Shoot();
			
			_holdingDownPrimary = true;
		}
		else
			_holdingDownPrimary = false;
		
		if(Input.IsActionPressed("reload"))
			Reload();
	}


	private void Enter()
	{
		_animationPlayer.Queue(_currentWeapon.ActivateAnim);
		_actionCooldown = _currentWeapon.ActivateAnimLength;
	}

	private void Exit()
	{
		_animationPlayer.Queue(_currentWeapon.DeactivateAnim);
		_actionCooldown = _currentWeapon.DeactivateAnimLength;
	}

	private void Shoot()
	{
		if (_currentWeapon.CurrentAmmo <= 0)
		{
			OutOfAmmo();
			return;
		}

		_currentWeapon.CurrentAmmo--;
		
		_hitscanRay.ForceRaycastUpdate();
		if (_hitscanRay.IsColliding())
		{
			GD.Print("Collided with " + _hitscanRay.GetCollider());
			
			var hitIndicator = _debugBulletDecal.Instantiate() as Node3D;
			GetTree().Root.AddChild(hitIndicator);
			hitIndicator.GlobalPosition = _hitscanRay.GetCollisionPoint();
		}
		else
			GD.Print("Missed the shot.");

		EmitSignal(SignalName.AmmoChanged, _currentWeapon);
		_animationPlayer.Queue(_currentWeapon.ShootAnim);
		_actionCooldown = _currentWeapon.ShootAnimLength;
	}

	private void Reload()
	{
		var ammoToLoad = Math.Min(_currentWeapon.ReserveAmmo, _currentWeapon.Magazine - _currentWeapon.CurrentAmmo);
		if(ammoToLoad == 0) return;
		
		_currentWeapon.ReserveAmmo -= ammoToLoad;
		_currentWeapon.CurrentAmmo += ammoToLoad;
		EmitSignal(SignalName.AmmoChanged, _currentWeapon);
        
		_animationPlayer.Queue(_currentWeapon.ReloadAnim);
		_actionCooldown = _currentWeapon.ReloadAnimLength;
	}

	private void OutOfAmmo()
	{
		_animationPlayer.Queue(_currentWeapon.OutOfAmmoAnim);
		_actionCooldown = _currentWeapon.OutOfAmmoAnimLength;
	}

	private void ChangeWeapon()
	{
		if(_actionCooldown > 0.0) return;
		
		Exit();
		_currentWeapon = _weaponDictionary[_weaponList[_weaponIndicator]];
		Enter();

		EmitSignal(SignalName.WeaponChanged, _currentWeapon);
	}
}