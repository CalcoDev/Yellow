using Godot;
using System;
using System.Collections.Generic;

namespace Yellow.Components;

[GlobalClass]
public partial class SwingAreaComponent : Area3D
{
	public override void _Ready()
	{
		
	}
	
	public override void _Process(double delta)
	{
		
	}

	public List<HurtboxComponent> GetDamageable()
	{
		var damageableInSwingRange = new List<HurtboxComponent>();
		var areasInSwingRange = GetOverlappingAreas();

		foreach (var area in areasInSwingRange)
		{
			if(area is HurtboxComponent hc)
				damageableInSwingRange.Add(hc);
		}

		return damageableInSwingRange;
	}
}
