using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Yellow.Extensions;
using Yellow.Misc;
using Yellow.Resources;

namespace Yellow.Components;

[GlobalClass]
public partial class GroundCheckComponent : Area3D
{
    [ExportGroup("Settings")]
    [Export(PropertyHint.Layers3DPhysics)] public uint LayerMask;
    [Export] public GroundProperties DefaultGroundProperties;

    // Sth
    public bool IsOnGround { get; private set; }
    public bool WasOnGround { get; private set; }
    public GroundProperties GroundProperties { get; private set; }

    public bool IsOnMovingPlatform { get; private set; }
    public Vector3 MovingPlatformVelocity { get; private set; }
    private readonly HashSet<PhysicsBody3D> _movingPlatforms = new();

    [Signal]
    public delegate void OnEnterGroundEventHandler(PhysicsBody3D ground);

    [Signal]
    public delegate void OnExitGroundEventHandler(PhysicsBody3D ground);

    [Signal]
    public delegate void OnIsGroundedEventHandler();

    [Signal]
    public delegate void OnIsNotGroundedEventHandler();

    private readonly List<PhysicsBody3D> _colls = new();
    private bool _touchingGround = false;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        ProcessPriority = (int)NodeProcessOrder.GroundChecks;

        GroundProperties = DefaultGroundProperties;
    }

    public override void _Process(double delta)
    {
        // NOTE(calco): Cringe, I know, but it allows for better frame control
        WasOnGround = IsOnGround;
        if (IsOnGround != _touchingGround) {
            IsOnGround = _touchingGround;
            EmitSignal(IsOnGround ? SignalName.OnIsGrounded : SignalName.OnIsNotGrounded);
        }

        ComputeMovingPlatformVelocity();
        // TODO(calco): Maybe add some logic to remove unusable bodies.
        // TODO(calco): Also handle something to do with enemies
    }

    private bool IsCheckable(PhysicsBody3D body)
    {
        return (LayerMask & body.CollisionLayer) != 0;
    }

    private static bool IsStillUsable(PhysicsBody3D body)
    {
        return body.IsActive();
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is not PhysicsBody3D pBody || !IsCheckable(pBody) || pBody.IsInGroup("player")) {
            return;
        }
        _colls.Add(pBody);
        _touchingGround = true;

        var groundProps = pBody.GetFirstNodeOfType<GroundPropertiesComponent>();
        if (groundProps != null) {
            GroundProperties = groundProps.Properties;
        }

        EmitSignal(SignalName.OnEnterGround, pBody);

        if (IsMovingPlatform(pBody)) {
            IsOnMovingPlatform = true;
            if (_movingPlatforms.Count == 0) {
                MovingPlatformVelocity = GetMovingPlatformVelocity(pBody);
            }
            _movingPlatforms.Add(pBody);
        }

        // TODO(calco): Figure out if slope and the slope angle.
        // TODO(calco): Handle moving platforms
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is not PhysicsBody3D pBody || !IsCheckable(pBody) || pBody.IsInGroup("player")) {
            return;
        }
        _colls.Remove(pBody);

        for (int i = _colls.Count - 1; i >= 0; --i) {
            if (IsStillUsable(_colls[i])) {
                var groundProps = _colls[i].GetFirstNodeOfType<GroundPropertiesComponent>();
                if (groundProps != null) {
                    GroundProperties = groundProps.Properties;
                    break;
                }
            }
        }
        
        EmitSignal(SignalName.OnExitGround, pBody);

        if (IsMovingPlatform(pBody)) {
            _movingPlatforms.Remove(pBody);

            if (_movingPlatforms.Count == 0) {
                IsOnMovingPlatform = false;
            }
            if (_movingPlatforms.Count == 0) {
                MovingPlatformVelocity = GetMovingPlatformVelocity(pBody);
            }
            _movingPlatforms.Add(pBody);
        }

        if (_colls.Count == 0) {
            _touchingGround = false;
            GroundProperties = DefaultGroundProperties;
        }

        // TODO(calco): Handle moving platforms
    }

    public static bool IsMovingPlatform(Node3D body)
    {
        return body.IsInGroup("moving_platform");
    }

    public static Vector3 GetMovingPlatformVelocity(Node3D body)
    {
        if (body.IsInGroup("qodot_mover")) {
            return body.Get("MoverVelocity").As<Vector3>();
        }

        if (body is RigidBody3D rb) {
            return rb.LinearVelocity;
        }

        // TODO(calco): Handle characterbody3d or other stuff lol

        return Vector3.Zero;
    }

    private void ComputeMovingPlatformVelocity()
    {
        // TODO(calco): This was chosen compltely arbitrarily
        MovingPlatformVelocity = Vector3.Zero;
        if (IsOnMovingPlatform) {
            MovingPlatformVelocity = GetMovingPlatformVelocity(_movingPlatforms.Last());
        }
    }
}