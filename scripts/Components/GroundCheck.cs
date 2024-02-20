using System;
using System.Collections.Generic;
using Godot;
using Yellow.Extensions;
using Yellow.Misc;
using Yellow.Resources;

namespace Yellow.Components;

[GlobalClass]
public partial class GroundCheck : Area3D
{
    [ExportGroup("Settings")]
    [Export(PropertyHint.Layers2DPhysics)] private int LayerMask;

    [ExportGroup("Debug View")]
    [Export] public bool OnGround { get; private set; }
    [Export] public bool OnSlope { get; private set; }
    [Export] public float SlopeAngle { get; private set; }
    [Export] public GroundProperties GroundProperties { get; private set; }

    private bool _touchingGround = false;

    private readonly List<PhysicsBody3D> _colls = new();

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        ProcessPriority = (int)NodeProcessOrder.GroundChecks;
    }

    public override void _Process(double delta)
    {
        // NOTE(calco): Cringe, I know, but it allows for better frame control
        if (OnGround != _touchingGround) {
            OnGround = _touchingGround;
        }

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
        if (body is not PhysicsBody3D pBody || !IsCheckable(pBody)) {
            return;
        }
        _colls.Add(pBody);
        _touchingGround = true;

        var groundProps = pBody.GetFirstNodeOfType<GroundPropertiesComponent>();
        if (groundProps != null) {
            GroundProperties = groundProps.Properties;
        }

        // TODO(calco): Figure out if slope and the slope angle.
        
        // TODO(calco): Handle moving platforms
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is not PhysicsBody3D pBody || !IsCheckable(pBody)) {
            return;
        }
        _colls.Remove(pBody);

        for (int i = _colls.Count; i >= 0; --i) {
            if (IsStillUsable(_colls[i])) {
                var groundProps = _colls[i].GetFirstNodeOfType<GroundPropertiesComponent>();
                if (groundProps != null) {
                    GroundProperties = groundProps.Properties;
                    break;
                }
            }
        }

        if (_colls.Count == 0) {
            _touchingGround = false;
            GroundProperties = null;
        }

        // TODO(calco): Handle moving platforms
    }
}