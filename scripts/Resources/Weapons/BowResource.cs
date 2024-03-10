using Godot;
using System;

namespace Yellow.Resources.Weapons;

[GlobalClass]
public partial class BowResource : Resource
{
    [Export] public float Damage;
    [Export] public float ShotSpeed;
    
    [Export] public float ShootCooldown;
}
