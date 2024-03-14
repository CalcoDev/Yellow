using Godot;
using System;
using Yellow.Resources;

namespace Yellow.GameObjects.Weapons;

[GlobalClass]
public partial class Weapon : Node3D
{
	[Export] private protected Player Attacker;
	
	public override void _Ready()
	{
		
	}
	
	public override void _Process(double delta)
	{
		
	}

	public virtual double HandleInput(string inputName)
	{
		return 0.0;
	}

	public virtual double Equip()
	{
		Visible = true;
		return 0.0; 
	}

	public virtual double Unequip()
	{
		Visible = false;
		return 0.0; 
	}
}
