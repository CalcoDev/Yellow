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

	[Signal]
	public delegate void OnShootEventHandler(double cooldown);
	
	private Camera3D _camera;
	private float _cameraFovDefault;
	private int _chargeLevel = 0;
	private AnimationPlayer _animationPlayer;
	
	public override void _Ready()
	{
        _camera = Attacker.GetNode("Head/Camera/Camera") as Camera3D;
        _animationPlayer = GetNode("lowpoly_bow/AnimationPlayer") as AnimationPlayer;
        _cameraFovDefault = _camera!.Fov;
	}

	public override void _Process(double delta)
	{
		if (_chargeLevel > 0 && !Input.IsActionPressed("use_primary"))
			Shoot();
	}

	public override void HandleInput(string inputName)
	{
		switch (inputName)
		{
			case "use_primary": ChargeShot(); break;
		}
	}

	private void ChargeShot()
	{
		if (_chargeLevel == 0)
			_animationPlayer.Play("Armature_002Action");
			
		_chargeLevel = Math.Min(_data.ChargeMax, _chargeLevel + 1);
		
		if (_chargeLevel < _data.ChargeMax)
			_camera.Fov -= 2;
		else if(_chargeLevel == _data.ChargeMax)
			_animationPlayer.Pause();

		EmitSignal(Weapon.SignalName.OnActionWithCooldown, 0.1);
	}
	
	private void Shoot()
	{
		if (_arrow.Instantiate() is not Arrow arrowInstance) return;
		
		arrowInstance.Position = Attacker.Head.GlobalPosition + Attacker.Head.Forward().Normalized();
		arrowInstance.Trajectory = Attacker.Head.Forward() * _data.ShotSpeed * _chargeLevel;
		arrowInstance.Damage = _data.DamageMin + (_data.DamageMax - _data.DamageMin) / _data.ChargeMax * _chargeLevel;
		arrowInstance.Rotation = Attacker.Head.Rotation;
		
		GetTree().Root.AddChild(arrowInstance);

		EmitSignal(Weapon.SignalName.OnActionWithCooldown,
			Math.Max(0.4, _data.ShootCooldown * ((double)_chargeLevel / _data.ChargeMax)));
		EmitSignal(SignalName.OnShoot);
		
		_chargeLevel = 0;
		_camera.Fov = _cameraFovDefault;
		_animationPlayer.Play();
		_animationPlayer.Seek(1.3, true);
	}
}
