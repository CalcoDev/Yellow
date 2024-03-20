
using System;
using Godot;
using Yellow.Components;
using Yellow.Extensions;
using Yellow.Managers;

namespace Yellow.GameObjects.Projectiles;

public partial class LeStraightBoom : FirableProjectile
{
    [Export] private Area3D _area;
    [Export] private PackedScene _killfx;
    [Export] private HitboxComponent _hit;

    public override void _Ready()
    {
        _area.BodyEntered += OnBodyEntered;
        _hit.OnHit += OnHit;
    }

    private void OnHit(HurtboxComponent hurtbox, HitboxComponent hitbox)
    {
        SpawnFX();
    }

    private void OnBodyEntered(Node body)
    {
        SpawnFX();
    }

    private Vector3 _velocity = Vector3.Zero;

    public override void _PhysicsProcess(double delta)
    {
        LinearVelocity = _velocity;
    }

    private void SpawnFX()
    {
        QueueFree();
        var c = (1f - (Mathf.Clamp((Player.Instance.GlobalPosition - GlobalPosition).Length(), 0.05f, 5f) / 5f)) * 5f;
        Game.ActiveCamera.ShakeLength(c, 1, 0.2f, true);
        SoundManager.Instance.Play("explosion", position: GlobalPosition, volumeMult: 0.2f);
        
        if (_killfx == null) {
            return;
        }

        var e = _killfx.Instantiate<Node3D>();
        var parent = (Node)this;
        while (true) {
            var p2 = parent.GetParent();
            if (p2 == null) {
                break;
            }
            parent = p2;
        }
        parent.AddChild(e);
        CallDeferred(MethodName.Stuff, e, GlobalPosition);

    }

    private void Stuff(Node3D node, Vector3 pos)
    {
        node.GlobalPosition = pos;
        ((GpuParticles3D)node).Emitting = true;
    }

    public override void Shoot(Vector3 target, Vector3 curr, float maxHeight)
    {
        // ApplyImpulse();
        _velocity = (target + Vector3.Up * 1.5f - curr).Normalized() * maxHeight;
        // float gravity = 30f;

        // Vector3 d = target - curr;
        // float horizontalDistance = Mathf.Sqrt(d.X * d.X + d.Z * d.Z);
        // float time = horizontalDistance / Mathf.Sqrt(2 * maxHeight / gravity);
        // float verticalVelocity = Mathf.Sqrt(2 * gravity * maxHeight);
        // d /= 2f;
        // Vector3 initialVelocity = new (d.X, verticalVelocity, d.Z);

        // // Apply the impulse
        // Vector3 impulse = Mass * initialVelocity;
        // ApplyImpulse(impulse);
    }
}