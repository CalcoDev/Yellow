using Godot;
using System;
using Yellow.Extensions;
using Yellow.GameObjects;
using Yellow.GameObjects.Weapons;
using Yellow.Misc;
using Yellow.Resources;
using Yellow.Resources.Weapons;

[GlobalClass]
public partial class Bow : Weapon
{
	[Export] private BowResource _data;
	[Export] private PackedScene _arrow;

	private Marker3D _marker;
	
	public override void _Ready()
	{
        _marker = GetNode("Marker3D") as Marker3D;
	}

	public override void _Process(double delta)
	{
		
	}

	public override double HandleInput(string inputName)
	{
		return inputName switch
		{
			"use_primary" => Shoot(),
			_ => 0.0
		};
	}

	private double Shoot()
	{
		if (_arrow.Instantiate() is not Arrow arrowInstance) return 0.0;
		
		if (Attacker is Player player)
		{
			arrowInstance.Position = player.Head.GlobalPosition;

			arrowInstance.Trajectory = player.Head.Forward() * _data.ShotSpeed;
			arrowInstance.Rotation = player.Head.Rotation;
		}
		
		GetTree().Root.AddChild(arrowInstance);
		
		return _data.ShootCooldown;
	}
}
