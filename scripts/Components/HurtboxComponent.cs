using System.Collections.Generic;
using Godot;
using Yellow.Extensions;

namespace Yellow.Components;

public struct HitData
{
    public float Damage;
    public bool IgnoreIFrames;

    public HitData(float damage, bool ignoreIFrames)
    {
        Damage = damage;
        IgnoreIFrames = ignoreIFrames;
    }
}

[GlobalClass]
public partial class HurtboxComponent : Area3D
{
    [Export] public HealthComponent HealthComponent;
    [Export] public FactionComponent FactionComponent;
    [Export] public KnockBackComponent KnockBackComponent;

    [Export]
    public bool TakeContinuousDamage
    {
        get { return _takeContinuousDamage; }
        set
        {
            _takeContinuousDamage = value;

            if (_takeContinuousDamage)
                _invincibilityTimer = InvincibilityTime;
            else
                _invincibilityTimer = 0f;
        }
    }
    [Export] public float InvincibilityTime { get; set; } = 0f;

    private float _invincibilityTimer;

    [Signal]
    public delegate void OnHitEventHandler(HitboxComponent other);

    private readonly List<HitboxComponent> _hitboxes = new();
    private bool _takeContinuousDamage = false;

    public override void _Ready()
    {
        _invincibilityTimer = 0f;

        if (TakeContinuousDamage) {
            _invincibilityTimer = InvincibilityTime;
        }

        Connect(Area3D.SignalName.AreaEntered, new Callable(this, MethodName.OnAreaEntered));
        Connect(Area3D.SignalName.AreaExited, new Callable(this, MethodName.OnAreaExited));
    }

    private void CheckHitboxes()
    {
        // TODO(calco): This is probably not working. Just in case.
        foreach (HitboxComponent hitbox in _hitboxes)
        {
            // TODO(calco); is this or or and
            if (TakeContinuousDamage && hitbox.ContinuousDamage) {
                TryToHit(hitbox);
            }
        }
    }
    
    public void TryToHit(HitData hitData, KnockBackData kbData)
    {
        if(_invincibilityTimer > 0 && !hitData.IgnoreIFrames) return;
        
        HealthComponent?.TakeDamage(hitData.Damage);
        KnockBackComponent?.ApplyKnockBack(kbData);
        
        _invincibilityTimer = InvincibilityTime;
    }
    
    public void TryToHit(HitboxComponent hitbox)
    {
        bool sameFaction = FactionComponent.FactionType == hitbox.FactionComponent.FactionType;
        if (FactionComponent == null || sameFaction)
            return;
        
        bool a = hitbox.IgnoreInvincibility;
        bool b = !hitbox.IgnoreInvincibility && _invincibilityTimer <= 0f;
        bool c = !hitbox.IgnoreInvincibility && InvincibilityTime <= 0f;
        if (!(a || b || c)) return;
        
        EmitSignal(SignalName.OnHit, hitbox);
        hitbox?.EmitSignal(HitboxComponent.SignalName.OnHit, this, hitbox);
        HealthComponent?.TakeDamage(hitbox.Damage);
        _invincibilityTimer = InvincibilityTime;
    }

    private void OnInvincibilityTimerTimeout()
    {
        _invincibilityTimer = 0f;

        CheckHitboxes();
    }

    private void OnAreaEntered(Node body)
    {
        HitboxComponent hitbox = body.GetFirstNodeOfType<HitboxComponent>();

        if (hitbox == null || _hitboxes.Contains(hitbox))
            return;

        _hitboxes.Add(hitbox);
        TryToHit(hitbox);
    }

    private void OnAreaExited(Node body)
    {
        HitboxComponent hitbox = body.GetFirstNodeOfType<HitboxComponent>();

        if (hitbox == null || !_hitboxes.Contains(hitbox))
            return;

        _hitboxes.Remove(hitbox);
    }
}