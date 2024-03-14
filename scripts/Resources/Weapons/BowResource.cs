using Godot;
using System;

namespace Yellow.Resources.Weapons;

[GlobalClass]
public partial class BowResource : Resource
{
    [Export] public float DamageMin;
    [Export] public float DamageMax;
    [Export] public int ChargeMax;
    [Export] public float ShotSpeed;
    [Export] public float ShootCooldown;
}
