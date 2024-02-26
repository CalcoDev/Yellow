using System;
using Godot;
using Yellow.Managers;
using Yellow.Misc;

namespace Yellow.Components;

[GlobalClass]
public partial class PathfindingComponent : Node3D
{
    [ExportGroup("Settings")]
    [Export] public float RecomputeInterval = 0.5f;
    private float _recomputeTimer = -1f;

    [Node("NavigationAgent3D")] private NavigationAgent3D _nav;

    public bool IsFinished => _nav.IsNavigationFinished();
    public Vector3 TargetPosition
    {
        get => _targetPos;
        set {
            if (_recomputeTimer > 0.0f) {
                return;
            }
            _targetPos = value;
            _nav.TargetPosition = _targetPos;
            _recomputeTimer = RecomputeInterval;
        }
    }
    private Vector3 _targetPos;

    public Vector3 CachedDir { get; private set; } = Vector3.Zero;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated || what == NotificationEnterTree) {
            this.WireNodes();
        }
    }

    public override void _Ready()
    {
        _nav.VelocityComputed += OnVelocityComputed;
    }

    public override void _Process(double delta)
    {
        _recomputeTimer -= Game.DeltaTime;
    }

    public void FollowPath()
    {
        if (_nav.IsNavigationFinished()) {
            CachedDir = Vector3.Zero;
            return;
        }

        var dir = (_nav.GetNextPathPosition() - GlobalPosition).Normalized();
        _nav.Velocity = dir;
    }

    private void OnVelocityComputed(Vector3 safeVelocity)
    {
        CachedDir = safeVelocity.Normalized();
        GD.Print("vel: ", safeVelocity);
    }
}