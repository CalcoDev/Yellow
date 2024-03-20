using Godot;
using System;

namespace Yellow.Resources.Projectiles;

[GlobalClass]
public partial class ArrowResource : Resource
{
    [Export] public float StartingSpeed;
}
