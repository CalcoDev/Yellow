using System.Collections.Generic;
using Godot;
using Yellow.Misc;
using Yellow.Resources;

namespace Yellow.Components;

// TODO(calco): ADD A WAY TO FILTER OUT BODIES ETCETC
[GlobalClass]
public partial class ShapeCastComponent : Node3D
{
    [ExportGroup("Settings")]
    [Export(PropertyHint.Layers3DPhysics)] public uint LayerMask;
    [Export] public float SlopeAngleThreshold;
    [Export] public bool SlopeAngleThresholdLess;

    [Signal]
    public delegate void OnIsCollidingEventHandler();

    [Signal]
    public delegate void OnIsNotCollidingEventHandler();

    // Stuff
    public bool IsColliding { get; private set; }
    public bool WasColliding { get; private set; }
    public bool IsOnSlope { get; private set; }
    
    public float SlopeAngle { get; private set; }
    public Vector3 ClosestNormal { get; private set; }
    public Vector3 ClosestPoint { get; private set; }
    
    private bool _isColliding = false;
    private readonly List<ShapeCast3D> _shapeCasts = new();

    public override void _Ready()
    {
        ProcessPriority = (int)NodeProcessOrder.ShapeChecks;
        
        for (int i = 0; i < GetChildCount(); ++i) {
            var child = GetChild(i);
            if (child is not ShapeCast3D shapeCast) {
                continue;
            }

            _shapeCasts.Add(shapeCast);
            shapeCast.CollisionMask = LayerMask;
        }

        if (_shapeCasts.Count == 0) {
            IsColliding = false;
            _isColliding = false;
            
            IsOnSlope = false;
            SlopeAngle = 0f;
           
            ClosestPoint = Vector3.Zero;
            ClosestNormal = Vector3.Zero;
        }
    }

    public override void _Process(double delta)
    {
        // NOTE(calco): Cringe, I know, but it allows for better frame control
        if (IsColliding != _isColliding) {
            IsColliding = _isColliding;
            EmitSignal(IsColliding ? SignalName.OnIsColliding: SignalName.OnIsNotColliding);
        }

        // NOTE(calco): Time to do the stuff
        if (_shapeCasts.Count == 0) {
            return;
        }

        (Vector3 point, Vector3 normal, Vector3 rayDir) closestColInfo = new();
        IsColliding = false;
        foreach (var shape in _shapeCasts) {
            var cnt = shape.GetCollisionCount();
            var rayDir = shape.TargetPosition;
            IsColliding |= cnt > 0;
            for (int i = 0; i < cnt; ++i) {
                var normal = shape.GetCollisionNormal(i);
                var point = shape.GetCollisionPoint(i);
                
                var dist = GlobalPosition.DistanceSquaredTo(point);
                var minDist = GlobalPosition.DistanceSquaredTo(closestColInfo.point);
                if (dist < minDist) {
                    closestColInfo = (point, normal, rayDir);
                }
            }
        }

        if (IsColliding) {
            ClosestPoint = closestColInfo.point;
            ClosestNormal = closestColInfo.normal.Normalized();
            SlopeAngle = Mathf.RadToDeg(ClosestNormal.AngleTo(Vector3.Up));
            IsOnSlope = Mathf.Abs(SlopeAngle) >= 0.005f && SlopeAngleThresholdLess ? SlopeAngle < SlopeAngleThreshold : SlopeAngle > SlopeAngleThreshold;
        } else {
            IsOnSlope = false;
        }

        WasColliding = IsColliding;
    }
}