using Godot;
using System;

namespace Yellow.Resources;

[GlobalClass]
public partial class SwordResource : Resource
{
	[Export] public float Damage;
	[Export] public float KnockBackFactor = 1.0f;

	[Export] public string HorizontalSwingAnim;
	[Export] public string VerticalSwingAnim;

	[Export] public float HorizontalSwingAnimDuration;
	[Export] public float VerticalSwingAnimDuration;
}
