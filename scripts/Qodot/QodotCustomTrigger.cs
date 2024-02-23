using Godot;
using Godot.Collections;
using System;

namespace Yellow.Qodot;

public partial class QodotCustomTrigger : Area3D
{
	[Export] public Dictionary properties;

	public override void _Ready()
	{
	}
}
