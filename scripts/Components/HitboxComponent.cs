using Godot;

namespace Yellow.Components;

[GlobalClass]
public partial class HitboxComponent : Area3D
{
    [Export] public float Damage { get; set; }
    [Export] public bool IgnoreInvincibility { get; set; } = false;
    [Export] public bool ContinuousDamage { get; set; } = false;
    [Export] public FactionComponent FactionComponent;

    [Signal]
    public delegate void OnHitEventHandler(HurtboxComponent hurtbox, HitboxComponent hitbox);
}