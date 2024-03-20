using System;
using Godot;
using Yellow.Components;
using Yellow.Managers;

namespace Yellow.GameObjects.Projectiles;

public partial class LeBoom : FirableProjectile
{
    [Export] private Area3D _area;
    [Export] private PackedScene _epxlosion;

    public override void _Ready()
    {
        _area.BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        // GD.Print("aaa");
        var e = _epxlosion.Instantiate<Node3D>();
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
        QueueFree();

        var c = (1f - (Mathf.Clamp((Player.Instance.GlobalPosition - GlobalPosition).Length(), 0.05f, 25f) / 25f)) * 10f;
        Game.ActiveCamera.ShakeLength(c, 1, 0.2f, true);
        SoundManager.Instance.Play("explosion", position: GlobalPosition);
    }

    private void Stuff(Node3D node, Vector3 pos)
    {
        node.GlobalPosition = pos;
        ((GpuParticles3D)node).Emitting = true;
    }

    public override void Shoot(Vector3 target, Vector3 curr, float maxHeight)
    {
        float gravity = 30f;

        Vector3 d = target - curr;
        float horizontalDistance = Mathf.Sqrt(d.X * d.X + d.Z * d.Z);
        float time = horizontalDistance / Mathf.Sqrt(2 * maxHeight / gravity);
        float verticalVelocity = Mathf.Sqrt(2 * gravity * maxHeight);
        d /= 2f;
        Vector3 initialVelocity = new (d.X, verticalVelocity, d.Z);

        // Apply the impulse
        Vector3 impulse = Mass * initialVelocity;
        ApplyImpulse(impulse);
        // GD.Print(impulse);
    }
}