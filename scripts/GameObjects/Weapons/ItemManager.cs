using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using Godot.Collections;

namespace Yellow.GameObjects.Weapons;

public partial class ItemManager : Node
{
	private List<Weapon> _allWeapons = new();
	private Weapon _currentWeapon;
	private int _weaponIndex = 0;

	private List<string> _weaponInputs;
	private List<string> _managerInputs;

	private double _actionCooldown = 0.0;
	
	public override void _Ready()
	{
		var children = GetChildren();
		
		foreach(var child in children)
			if(child is Weapon weapon)
				_allWeapons.Add(weapon);
		
		GD.Print("We have " + _allWeapons.Count + " weapons");

		_weaponInputs = new List<string>
		{
			"use_primary",
			"use_secondary",
			"reload"
		};

		_managerInputs = new List<string>
		{
			"weapon_up",
			"weapon_down"
		};

		_currentWeapon = _allWeapons[0];
	}

	public override void _Process(double delta)
	{
		_actionCooldown = Math.Max(0, _actionCooldown - delta);
	}
	
	public override void _UnhandledInput(InputEvent inputEvent)
	{	
		if(_actionCooldown > 0) return;
		
		foreach(var i in _managerInputs)
			if (inputEvent.IsActionPressed(i))
			{
				GD.Print("Handling input " + i);
				HandleInput(i);
			}
		
		if(_currentWeapon == null) return;
		foreach(var i in _weaponInputs)
			if(inputEvent.IsActionPressed(i))
			{
				GD.Print("Handling input " + i);
				_actionCooldown = _currentWeapon.HandleInput(i);
			}
	}

	private void HandleInput(string inputName)
	{
		switch (inputName)
		{
			case "weapon_up": ScrollToNext(); break;
			case "weapon_down": ScrollToPrev(); break;
		}
	}

	private void ScrollToNext()
	{
		if (_weaponIndex + 1 >= _allWeapons.Count)
			_weaponIndex = 0;
		else
			_weaponIndex++;

		SwitchWeapon();
	}

	private void ScrollToPrev()
	{
		if (_weaponIndex - 1 < 0)
			_weaponIndex = _allWeapons.Count - 1;
		else
			_weaponIndex--;
		
		SwitchWeapon();
	}

	private void SwitchWeapon()
	{
		_actionCooldown += _currentWeapon.Unequip();
		_currentWeapon = _allWeapons[_weaponIndex];
		_actionCooldown += _currentWeapon.Equip();
		GD.Print("Scrolled to slot " + _weaponIndex + ", to weapon " + _currentWeapon.Name);
	}
}
