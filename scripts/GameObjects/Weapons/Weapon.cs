using Godot;
using System;
using Yellow.Resources;

namespace Yellow.GameObjects.Weapons;

[GlobalClass]
public partial class Weapon : Node3D
{
	[Export] private protected Player Attacker;

	[Signal]
	public delegate void OnActionWithCooldownEventHandler(double cooldown);
	
	public override void _Ready()
	{
		
	}
	
	public override void _Process(double delta)
	{
		
	}

	public virtual void HandleInput(string inputName)
	{
		
	}

	public virtual void Equip()
	{
		Visible = true;
		EmitSignal(SignalName.OnActionWithCooldown, 0.5);
	}

	public virtual void Unequip()
	{
		Visible = false;
	}
}
