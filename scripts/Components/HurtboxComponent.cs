using System.Collections.Generic;
using System.Linq;
using Godot;
using Yellow.Extensions;
using Yellow.Managers;

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
    [Export] public float InvincibilityTime { get; set; } = 0f;
    [Export]
    public bool TakeContinuousDamage
    {
        get => _takeContinuousDamage;
        set
        {
            _takeContinuousDamage = value;
            _invincibilityTimer = _takeContinuousDamage ? InvincibilityTime : 0f;
        }
    }
    [Signal]
    public delegate void OnHitEventHandler(HitboxComponent other);

    private float _invincibilityTimer;
    private readonly List<HitboxComponent> _hitboxes = new();
    private bool _takeContinuousDamage = false;

    public override void _Ready()
    {
        _invincibilityTimer = 0f;

        if (TakeContinuousDamage) 
            _invincibilityTimer = InvincibilityTime;

        Connect(Area3D.SignalName.AreaEntered, new Callable(this, MethodName.OnAreaEntered));
        Connect(Area3D.SignalName.AreaExited, new Callable(this, MethodName.OnAreaExited));
    }

    private void CheckHitboxes()
    {
        foreach (var hitbox in _hitboxes.Where(hitbox => TakeContinuousDamage && hitbox.ContinuousDamage))
            TryToHit(hitbox);
    }
    
    public void TryToHit(HitData hitData, KnockBackData kbData)
    {
        if(_invincibilityTimer > 0 && !hitData.IgnoreIFrames) return;
        
        HealthComponent?.TakeDamage(hitData.Damage);
        GD.Print("Damged: ", HealthComponent, " with ", hitData.Damage);
        KnockBackComponent?.ApplyKnockBack(kbData);
        
        _invincibilityTimer = InvincibilityTime;
        EmitSignal(SignalName.OnHit, null);
    }
    
    public void TryToHit(HitboxComponent hitbox)
    {
        var sameFaction = FactionComponent.FactionType == hitbox.FactionComponent.FactionType;
        if (FactionComponent == null || sameFaction)
            return;
        
        var a = hitbox.IgnoreInvincibility;
        var b = !hitbox.IgnoreInvincibility && _invincibilityTimer <= 0f;
        var c = !hitbox.IgnoreInvincibility && InvincibilityTime <= 0f;
        if (!(a || b || c)) return;
        
        EmitSignal(SignalName.OnHit, hitbox);
        hitbox?.EmitSignal(HitboxComponent.SignalName.OnHit, this, hitbox);
        HealthComponent?.TakeDamage(hitbox.Damage);
        _invincibilityTimer = InvincibilityTime;
    }

    public override void _Process(double delta)
    {
        _invincibilityTimer -= Game.DeltaTime;
    }

    private void OnInvincibilityTimerTimeout()
    {
        _invincibilityTimer = 0f;

        CheckHitboxes();
    }

    private void OnAreaEntered(Node body)
    {
        var hitbox = (body is HitboxComponent component ? component : body.GetFirstNodeOfType<HitboxComponent>());
        if (hitbox == null || _hitboxes.Contains(hitbox)) return;

        _hitboxes.Add(hitbox);
        TryToHit(hitbox);
    }

    private void OnAreaExited(Node body)
    {
        var hitbox = (body is HitboxComponent component ? component : body.GetFirstNodeOfType<HitboxComponent>());
        if (hitbox == null || _hitboxes.Contains(hitbox)) return;

        _hitboxes.Remove(hitbox);
    }
}