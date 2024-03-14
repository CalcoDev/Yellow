using Godot;
using System;
using Yellow.Extensions;
using Yellow.GameObjects.Projectiles;
using Yellow.Resources.Weapons;

namespace Yellow.GameObjects.Weapons;

[GlobalClass]
public partial class Bow : Weapon
{
	[Export] private BowResource _data;
	[Export] private PackedScene _arrow;
	
	private Camera3D _camera;
	private float _cameraFovDefault;
	private int _chargeLevel = 0;
	
	public override void _Ready()
	{
        _camera = Attacker.GetNode("Head/Camera/Camera") as Camera3D;
        _cameraFovDefault = _camera.Fov;
	}

	public override void _Process(double delta)
	{
		if (_chargeLevel > 0 && !Input.IsActionPressed("use_primary"))
			Shoot();
	}

	public override double HandleInput(string inputName)
	{
		return inputName switch
		{
			"use_primary" => ChargeShot(),
			_ => 0.0
		};
	}

	private double ChargeShot()
	{
		_chargeLevel = Math.Min(_data.ChargeMax, _chargeLevel + 1);
		if (_chargeLevel < _data.ChargeMax)
			_camera.Fov -= 2;
		
		return 0.1;
	}
	
	private double Shoot()
	{
		if (_arrow.Instantiate() is not Arrow arrowInstance) return 0.0;
		
		arrowInstance.Position = Attacker.Head.GlobalPosition + Attacker.Head.Forward().Normalized();
		arrowInstance.Trajectory = Attacker.Head.Forward() * _data.ShotSpeed * _chargeLevel;
		arrowInstance.Damage = _data.DamageMin + (_data.DamageMax - _data.DamageMin) / _data.ChargeMax * _chargeLevel;
		arrowInstance.Rotation = Attacker.Head.Rotation;
		
		GetTree().Root.AddChild(arrowInstance);

		_chargeLevel = 0;
		_camera.Fov = _cameraFovDefault;
		
		return _data.ShootCooldown;
	}
}
