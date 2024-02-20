using Godot;
using Yellow.Components;
using Yellow.Extensions;
using Yellow.Managers;
using Yellow.Misc;
using Yellow.Resources;

namespace Yellow.GameObjects;

using static Yellow.Resources.PlayerInput;

public partial class Player : RigidBody3D
{
    [ExportGroup("References")]
    [Export] private PlayerInput _input;
    [Export] private GroundCheckComponent _groundCheck;

    [Node("Head")] private Node3D _head;
    [Node("Head/Camera")] private Camera3D _camera;

    [ExportGroup("Movement")]
    [Export] private float Sensitivity;

    [ExportSubgroup("Run")]
    [Export] private float RunSpeed;
    [Export] private float AirAccelMult;

    [ExportSubgroup("Jump")]
    [Export] private float JumpForce;
    [Export] private float JumpReleaseMult;

    // States
    public bool IsJumping { get; private set; } = false;

    // Input
    private Vector3 _moveDir;
    private Vector3 _moveDir2;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated || what == NotificationEnterTree) {
            this.WireNodes();
        }
    }

    public override void _Ready()
    {
        ProcessPriority = (int)NodeProcessOrder.Player;
        AddToGroup("player");

        _groundCheck.OnIsNotGrounded += () => {
            IsJumping = false;
        };
    }

    public override void _Input(InputEvent e)
    {
        if (!Game.MouseLocked) {
            return;
        }

        if (e is InputEventMouseMotion motion) {
            _head.RotateY(-motion.Relative.X * (Sensitivity - 40) / 10000f);
            _camera.RotateX(motion.Relative.Y * (Sensitivity - 40) / 10000f);
            
            const float MIN_ROT = -80f * (Mathf.Pi / 180f);
            const float MAX_ROT = 80f * (Mathf.Pi / 180f);
            _camera.Rotation = new (
                Mathf.Clamp(_camera.Rotation.X, MIN_ROT, MAX_ROT),
                _camera.Rotation.Y,
                _camera.Rotation.Z
            );
        }
    }
    public override void _Process(double delta)
    {
        if (_input.Jump == KeyState.Pressed) {
            ApplyImpulse(Vector3.Up * JumpForce);
            IsJumping = true;
        }

        var right = _head.GlobalTransform.Basis.X * -_input.Movement.X;
        var forward = _head.GlobalTransform.Basis.Z * _input.Movement.Y;
        _moveDir = (forward + right).Normalized();
    }

    private Vector3 _diffDir;
    private bool _diff;
    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        _moveDir2 = _moveDir * Game.FixedDeltaTime * RunSpeed;
        if (_groundCheck.IsOnGround && !IsJumping) {
            GravityScale = _moveDir.LengthSquared() < 0.01f ? 0 : 1;

            float y = LinearVelocity.Y;
            float t = 0.25f * (1f + _groundCheck.GroundProperties.Friction);
            state.LinearVelocity = state.LinearVelocity.Lerp(_moveDir2, t);
            
            state.LinearVelocity = new(state.LinearVelocity.X, y, state.LinearVelocity.Z);
            state.SetConstantForce(Vector3.Zero);

            _diff = false;
        }
        else {
            GravityScale = 1;
            var vel = state.LinearVelocity;

            var cX = (_moveDir2.X > 0 && vel.X < _moveDir2.X) || (_moveDir2.X < 0 && vel.X > _moveDir2.X);
            var cZ = (_moveDir2.Z > 0 && vel.Z < _moveDir2.Z) || (_moveDir2.Z < 0 && vel.Z > _moveDir2.Z);

            var airForce = new Vector3(cX ? _moveDir2.X : 0, _moveDir2.Y, cZ ? _moveDir2.Z : 0) * AirAccelMult;

            var vel0 = state.LinearVelocity.WithY(0);
            if (vel0.Normalized().DotLess(_moveDir, 0.25f)) {
                var f = -vel0 / Game.FixedDeltaTime * 0.25f;
                state.ApplyForce(f);
            }
            if (vel0.Normalized().DotLess(_moveDir, 0.9f)) {
                var f = _moveDir2 - vel0;
                state.ApplyForce(f);
            }
            // if (_diff) {
            //     var vel0Norm = vel0.Normalized();
            //     if (vel0Norm.DotLess(_diffDir, 0.75f) || vel0Norm.DotGreater(_moveDir, 0.75f, false)) {
            //         _diff = false;
            //     }
            // } else {
            //     if (vel0.Normalized().DotLess(_moveDir, 0.25f)) {
            //         _diff = true;
            //         _diffDir = vel0;
            //     }
            // }
            // if (_diff) {
            //     var f = -_diffDir / Game.FixedDeltaTime * 0.25f;
            //     state.ApplyForce(f);
            // }

            state.ApplyForce(airForce);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
    }
}