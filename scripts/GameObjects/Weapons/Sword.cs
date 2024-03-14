using System;
using System.Collections.Generic;
using Godot;
using Yellow.Components;
using Yellow.Misc;
using Yellow.Resources;

namespace Yellow.GameObjects.Weapons;

[GlobalClass]
public partial class Sword : Weapon
{
	[Export] private SwordResource _data;
	[Export] private SwingAreaComponent _swingArea;
	[Export] private AnimationPlayer _animation;
	[Export] private CameraComponent _playerCamera;
	
	private int _comboCounter = 0;
	private double _comboTimer = 0.0;
	
	public override void _Ready()
	{
        
	}

	public override void _Process(double delta)
	{
		_comboTimer = Math.Max(0, _comboTimer - delta);
		//if (_comboTimer == 0 && _comboCounter > 0)
		//{
		//	_animation.Queue(_data.ReturnAnim);
		//	_comboCounter = 0;
		//}
	}

	public override double HandleInput(string inputName)
	{
		return inputName switch
		{
			"use_primary" => HorizontalSwing(),
			"use_secondary" => VerticalSwing(),
			_ => 0.0
		};
	}

	private double HorizontalSwing()
	{
		var hitSomething = TryToHit(
			new HitData(_data.Damage, false),
			new KnockBackData(Attacker, new Vector3(2.5f, 1.0f, 2.5f), _data.KnockBackFactor)
			);

		if (hitSomething)
		{
			_playerCamera.ShakeLength(10f, 3f, 0.2f, true);
			_comboTimer = _data.VerticalSwingDuration;
		}
			
		_animation.Queue(_comboCounter % 2 == 0 ? _data.HorizontalSwingLeft : _data.HorizontalSwingRight);

		return _data.HorizontalSwingDuration;
	}

	private double VerticalSwing()
	{
		TryToHit(
			new HitData(_data.Damage, false),
			new KnockBackData(Attacker, new Vector3(0.6f, 8.5f, 0.6f), _data.KnockBackFactor)
		);
		
		//_animation.Queue(Data.VerticalSwingAnim);
		return _data.VerticalSwingDuration;
	}

	private bool TryToHit(HitData hitData, KnockBackData kbData)
	{
		var hbToDamage = _swingArea.GetDamageable();
		if (hbToDamage.Count == 0)
		{
			_comboCounter = 0;
			return false;
		}

		_comboCounter++;
		foreach (var hb in hbToDamage)
			hb.TryToHit(
				hitData, 
				kbData
			);
		return true;
	}
}
