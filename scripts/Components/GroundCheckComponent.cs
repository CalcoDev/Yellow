using System;
using System.Collections.Generic;
using Godot;
using Yellow.Extensions;
using Yellow.Misc;
using Yellow.Resources;

namespace Yellow.Components;

[GlobalClass]
public partial class GroundCheckComponent : Area3D
{
    [ExportGroup("Settings")]
    [Export(PropertyHint.Layers3DPhysics)] public int LayerMask;
    [Export] public GroundProperties DefaultGroundProperties;
    [Export] public bool DetectSlope;

    // Sth
    public bool IsOnGround { get; private set; }
    public bool OnSlope { get; private set; }
    public float SlopeAngle { get; private set; }
    public GroundProperties GroundProperties { get; private set; }
    public Vector3 ClosestPoint { get; private set; }

    [Signal]
    public delegate void OnEnterGroundEventHandler(PhysicsBody3D ground);

    [Signal]
    public delegate void OnExitGroundEventHandler(PhysicsBody3D ground);

    [Signal]
    public delegate void OnIsGroundedEventHandler();

    [Signal]
    public delegate void OnIsNotGroundedEventHandler();

    private bool _touchingGround = false;

    private readonly List<PhysicsBody3D> _colls = new();
    
    // NOTE(calco): Class for reference passing
    private class ShapeInfo : IEquatable<ShapeInfo>
    {
        public CollisionShape3D Owner;
        public Shape3D Shape;

        public ShapeInfo(CollisionShape3D owner, Shape3D shape)
        {
            Owner = owner;
            Shape = shape;
        }

        public override bool Equals(object obj)
        {
            return base.Equals((ShapeInfo)obj);
        }

        public bool Equals(ShapeInfo other)
        {
            return other.Owner == Owner && other.Shape == Shape;
        }

        public Transform3D GetOwnerTransform()
        {
            return Owner.GlobalTransform;
        }
    }
    private readonly List<ShapeInfo> _shapes = new();
    private ShapeInfo _selfShapeInfo;

    public override void _Ready()
    {
        BodyShapeEntered += OnShapeEntered;
        BodyShapeExited += OnShapeExited;

        ProcessPriority = (int)NodeProcessOrder.GroundChecks;

        GroundProperties = DefaultGroundProperties;
    }

    public override void _Process(double delta)
    {
        // NOTE(calco): Cringe, I know, but it allows for better frame control
        if (IsOnGround != _touchingGround) {
            IsOnGround = _touchingGround;
            EmitSignal(IsOnGround ? SignalName.OnIsGrounded : SignalName.OnIsNotGrounded);
        }

        if (DetectSlope) {
            HandleSlope();
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

    private static ShapeInfo GetShapeInfo(CollisionObject3D obj, int shapeIdx)
    {
        var shapeOwnerId = obj.ShapeFindOwner(shapeIdx);
        var shapeOwner = obj.ShapeOwnerGetOwner(shapeOwnerId);
        return new ShapeInfo((CollisionShape3D)shapeOwner, obj.ShapeOwnerGetShape(shapeOwnerId, 0));
    }
    
    private void OnShapeEntered(Rid bodyRid, Node3D body, long bodyShapeIndex, long localShapeIndex)
    {
        if (body is not PhysicsBody3D pBody || !IsCheckable(pBody) || pBody.IsInGroup("player") || _colls.Contains(pBody)) {
            return;
        }

        var bodyShapeInfo = GetShapeInfo((CollisionObject3D)body, (int)bodyShapeIndex);
        var areaShapeInfo = GetShapeInfo(this, (int)localShapeIndex);
        if (_selfShapeInfo == null) {
            _selfShapeInfo = areaShapeInfo;
        } else if (areaShapeInfo != null && !_selfShapeInfo.Equals(areaShapeInfo)) {
            GD.PushError("ERROR: GroundChecker ENTER different self shape ayo?");
            _selfShapeInfo = areaShapeInfo;
        }
        _shapes.Add(bodyShapeInfo);
        
        _colls.Add(pBody);
        _touchingGround = true;

        var groundProps = pBody.GetFirstNodeOfType<GroundPropertiesComponent>();
        if (groundProps != null) {
            GroundProperties = groundProps.Properties;
        }

        EmitSignal(SignalName.OnEnterGround, pBody);

        // TODO(calco): Figure out if slope and the slope angle.
        // TODO(calco): Handle moving platforms
    }
   
    private void OnShapeExited(Rid bodyRid, Node3D body, long bodyShapeIndex, long localShapeIndex)
    {
        if (body is not PhysicsBody3D pBody || !IsCheckable(pBody) || pBody.IsInGroup("player") || !_colls.Contains(pBody)) {
            return;
        }

        var bodyShapeInfo = GetShapeInfo((CollisionObject3D)body, (int)bodyShapeIndex);
        var areaShapeInfo = GetShapeInfo(this, (int)localShapeIndex);
        if (_selfShapeInfo == null) {
            _selfShapeInfo = areaShapeInfo;
        } else if (areaShapeInfo != null && !_selfShapeInfo.Equals(areaShapeInfo)) {
            GD.PushError("ERROR: GroundChecker EXIT different self shape ayo?");
            _selfShapeInfo = areaShapeInfo;
        }
        _shapes.Remove(bodyShapeInfo);
        _colls.Remove(pBody);

        // TODO(calco): Useless, as we check shape exit.
        // for (int i = _shapes.Count - 1; i >= 0; --i) {
        //     if (_shapes[i].Owner == pBody) {
        //         _shapes.RemoveAt(i);
        //     }
        // }

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

        if (_colls.Count == 0) {
            _touchingGround = false;
            GroundProperties = DefaultGroundProperties;
        }

        // TODO(calco): Handle moving platforms
    }

    private void HandleSlope()
    {
        if (_selfShapeInfo == null) {
            return;
        }

        foreach (var shape in _shapes) {
            // _selfShapeInfo.Shape.
        }
        // GD.Print(string.Join(", ", _shapes));

        // Figure out: closest point
        // then raycast from body to closest point, colliding with body and get slope to that???
    }
}