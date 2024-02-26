using Godot;
using System;

namespace Yellow.Resources.Items;

[GlobalClass]
public partial class ItemResource : Resource
{
	[Export] public string DisplayName = "Default";
	[Export] public float Cooldown = 1.0f;
}