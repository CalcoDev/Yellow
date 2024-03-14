using Godot;
using System;

namespace Yellow.Resources;

[GlobalClass]
public partial class SwordResource : Resource
{
	[Export] public float Damage;
	[Export] public float KnockBackFactor = 1.0f;

	[Export] public string HorizontalSwingLeft;
	[Export] public string HorizontalSwingRight;
	[Export] public string VerticalSwingAnim;
	[Export] public string ReturnAnim;

	[Export] public float HorizontalSwingDuration;
	[Export] public float VerticalSwingDuration;
	[Export] public float ReturnDuration;
}
