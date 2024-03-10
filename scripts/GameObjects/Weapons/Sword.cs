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
	
	[Node("Animation")] 
	private AnimationPlayer _animation;
	
	public override void _Ready()
	{
        
	}

	public override void _Process(double delta)
	{
        
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
		TryToHit(
			new HitData(_data.Damage, false),
			new KnockBackData(Attacker, new Vector3(2.5f, 1.0f, 2.5f), _data.KnockBackFactor)
			);
			
		//_animation.Queue(Data.HorizontalSwingAnim);
		return _data.HorizontalSwingAnimDuration;
	}

	private double VerticalSwing()
	{
		TryToHit(
			new HitData(_data.Damage, false),
			new KnockBackData(Attacker, new Vector3(0.6f, 8.5f, 0.6f), _data.KnockBackFactor)
		);
		
		//_animation.Queue(Data.VerticalSwingAnim);
		return _data.VerticalSwingAnimDuration;
	}

	private void TryToHit(HitData hitData, KnockBackData kbData)
	{
		var hbToDamage = _swingArea.GetDamageable();

		foreach (var hb in hbToDamage)
			hb.TryToHit(
				hitData, 
				kbData
			);
	}
}
