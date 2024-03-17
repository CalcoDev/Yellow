using Godot;
using System;
using Yellow.Extensions;
using Yellow.GameObjects.Projectiles;
using Yellow.Managers;
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
	private MeshInstance3D _arrowModel;
	private double _arrowHideCooldown = 0;
	
	public override void _Ready()
	{
        _camera = Attacker.GetNode("Head/Camera/Camera") as Camera3D;
        _animationPlayer = GetNode("lowpoly_bow/AnimationPlayer") as AnimationPlayer;
        _arrowModel = GetNode("lowpoly_bow/Cube") as MeshInstance3D;
        _cameraFovDefault = _camera!.Fov;
	}

	public override void _Process(double delta)
	{
		_arrowHideCooldown = Math.Max(0, _arrowHideCooldown - delta);
		_arrowModel.Visible = (_arrowHideCooldown == 0);
		
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
		{
			_animationPlayer.Play("Armature_002Action");
			SoundManager.Instance.Play("PullBow");
		}
			
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
		arrowInstance.ChargePower = _chargeLevel;
		
		GetTree().Root.AddChild(arrowInstance);

		_arrowHideCooldown = Math.Max(0.4, _data.ShootCooldown * ((double)_chargeLevel / _data.ChargeMax));
		
		_animationPlayer.Play();
		_animationPlayer.Seek(1.3, true);
		
		EmitSignal(Weapon.SignalName.OnActionWithCooldown, _arrowHideCooldown);
		EmitSignal(SignalName.OnShoot);

		SoundManager.Instance.Play(_chargeLevel == _data.ChargeMax ? "ShootFullPower" : "ShootWeak");

		_chargeLevel = 0;
		_camera.Fov = _cameraFovDefault;
	}
}
