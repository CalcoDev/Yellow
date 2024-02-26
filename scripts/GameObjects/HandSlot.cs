using Godot;
using System;
public partial class HandSlot : Node
{
	[Export] public Gun HeldWeapon;
	public override void _Ready()
	{
		
	}
	public override void _Process(double delta)
	{
		
	}

	public void UsePrimary(Vector3 playerFacingDirection)
	{
		HeldWeapon.Shoot(playerFacingDirection);
	}

	public void UseSecondary()
	{
		HeldWeapon.Scope();
	}
}
