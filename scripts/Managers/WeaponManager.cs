using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Yellow.Misc;
using Yellow.Resources.Weapon;

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
	
	private uint _weaponIndicator = 0;
	private string _nextWeapon;
	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("Models/AnimationPlayer");
		Initialize();
	}
	public override void _Process(double delta)
	{
		
	}

	public override void _Input(InputEvent e)
	{
		if (e.IsActionPressed("weapon_up"))
		{
			_weaponIndicator++;
			_weaponIndicator %= (uint)_weaponList.Count;
			ChangeWeapon();
		}

		if (e.IsActionPressed("weapon_down"))
		{
			_weaponIndicator--;
			_weaponIndicator %= (uint)_weaponList.Count;
			ChangeWeapon();
		}
		
		if(e.IsActionPressed("use_primary")) 
			Shoot();
	}

	private void Initialize()
	{
		foreach (var weapon in WeaponResources)
			_weaponDictionary[weapon.WeaponName] = weapon;

		foreach (var weapon in StartWeapons)
			_weaponList.Add(weapon);

		_currentWeapon = _weaponDictionary[_weaponList[0]];
		Enter();
	}

	private void Enter()
	{
		_animationPlayer.Queue(_currentWeapon.ActivateAnim);
	}

	private void Exit()
	{
		_animationPlayer.Queue(_currentWeapon.DeactivateAnim);
	}

	private void Shoot()
	{
		_animationPlayer.Queue(_currentWeapon.ShootAnim);
	}

	private void ChangeWeapon()
	{
		Exit();
		_currentWeapon = _weaponDictionary[_weaponList[(int)_weaponIndicator]];
		Enter();
	}
}
