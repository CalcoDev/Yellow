using System;
using Godot;
using Yellow.Components;
using Yellow.Managers;
using Yellow.Resources.Weapons;

namespace Yellow.GameObjects.Weapons;

[GlobalClass]
public partial class Sword : Weapon
{
	[Export] private SwordResource _data;
	[Export] private SwingAreaComponent _swingArea;
	[Export] private AnimationPlayer _animation;
	[Export] private CameraComponent _playerCamera;

	[Signal]
	public delegate void OnHorizontalEventHandler(double cooldown);
	[Signal]
	public delegate void OnVerticalEventHandler(double cooldown);
	
	private int _comboCounter = 0;
	private double _comboTimer = 0.0;
	
	public override void _Ready()
	{
        
	}

	public override void _Process(double delta)
	{
		_comboTimer = Math.Max(0, _comboTimer - delta);
	}

	public override void HandleInput(string inputName)
	{
		switch (inputName)
		{
			case "use_primary": HorizontalSwing(); break;
			case "use_secondary": VerticalSwing(); break;
		}
	}

	private void HorizontalSwing()
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
		
		EmitSignal(Weapon.SignalName.OnActionWithCooldown, _data.HorizontalSwingDuration);
		EmitSignal(SignalName.OnHorizontal);
		_animation.Play(_data.HorizontalSwingAnim);
		SoundManager.Instance.Play("HorizontalSwing");
	}

	private void VerticalSwing()
	{
		TryToHit(
			new HitData(_data.Damage, false),
			new KnockBackData(Attacker, new Vector3(0.6f, 8.5f, 0.6f), _data.KnockBackFactor)
		);
		
		EmitSignal(Weapon.SignalName.OnActionWithCooldown, _data.VerticalSwingDuration);
		EmitSignal(SignalName.OnVertical);
		_animation.Play(_data.VerticalSwingAnim);
		SoundManager.Instance.Play("VerticalSwing");
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

		SoundManager.Instance.Play("SuccessfulHit");
		return true;
	}

	public override void Equip()
	{
		base.Equip();
		SoundManager.Instance.Play("SwordEquip");
	}
}
