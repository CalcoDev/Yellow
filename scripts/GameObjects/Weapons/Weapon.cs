using Godot;
using System;
using Yellow.Resources;

namespace Yellow.GameObjects.Weapons;

[GlobalClass]
public partial class Weapon : Node3D
{
	public override void _Ready()
	{
		
	}
	
	public override void _Process(double delta)
	{
		
	}

	public virtual double HandleInput(string inputName)
	{
		GD.Print("NOT swag");
		return 0.0;
	}

	public virtual double UsePrimary() { ; return 0.0; }

	public virtual double UseSecondary() { return 0.0; }
	
	public virtual double Equip() { return 0.0; }

	public virtual double Unequip() { return 0.0; }
}
