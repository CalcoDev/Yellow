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

    [ExportSubgroup("UI")]
    [Export] private PlayerUIManager _ui;

    [Node("Head")] private Node3D _head;

    [ExportGroup("Movement")]
    // TODO(calco): Move this to game manage or sth
    [Export] private float Sensitivity;

    [Export] private float MaxStamina = 3f;

    [ExportSubgroup("Run")]
    [Export] private float RunSpeed;
    [Export] private float AirAccelMult;

    [ExportSubgroup("Jump")]
    [Export] private float JumpForce;
    [Export] private float JumpReleaseMult;

    [ExportSubgroup("Dash")]
    [Export] private float DashStaminaCost = 1f;
    [Export] private float DashDuration = 0.2f;
    [Export] private float DashSpeed;
    [Export] private float DashEndSpeed;
    // [Export] private float 

    // States
    public bool IsJumping { get; private set; } = false;
    public bool IsDashing { get; private set; } = false;

    // Input
    private Vector3 _moveDir;
    private Vector3 _moveDir2;

    // Stamina
    private float _stamina;

    // Dash
    private Vector3 _dashDir;
    private float _dashTimer;

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
            var sens = Mathf.Lerp(0f, 1f, Sensitivity / 100f) / 350f;
            var x = -motion.Relative.X * sens;
            var y = motion.Relative.Y * sens;
            
            _head.RotateY(x);
            CameraManager.Instance.DoRotation(x, y);
        }
    }
    public override void _Process(double delta)
    {
        _stamina = Mathf.Clamp(_stamina + Game.DeltaTime, 0f, MaxStamina);

        // Jump
        if (_groundCheck.IsOnGround && !IsJumping && _input.Jump == KeyState.Pressed) {
            Jump();
        }

        // TODO(calco): Variable jump

        // Dash
        if (IsDashing) {
            _dashTimer -= Game.DeltaTime;
            if (_dashTimer < 0f) {
                StopDash();
            }
        } else if (_input.Dash == KeyState.Pressed && _stamina > DashStaminaCost) {
            _stamina -= DashStaminaCost;
            StartDash();
        }

        // Slide

        var right = _head.GlobalTransform.Basis.X * -_input.Movement.X;
        var forward = _head.GlobalTransform.Basis.Z * _input.Movement.Y;
        _moveDir = (forward + right).Normalized();

        _ui.DisplayStamina(_stamina);
    }

    private Vector3 _diffDir;
    private bool _diff;
    private bool _wasDashing;
    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        _moveDir2 = _moveDir * Game.FixedDeltaTime * RunSpeed;
        if (IsDashing) {
            GravityScale = 0f;
            state.LinearVelocity = _dashDir * DashSpeed;
        } 
        else if (_wasDashing) {
            _wasDashing = false;
            GravityScale = 1f;
            state.LinearVelocity = _dashDir * DashEndSpeed;
        }

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

            state.ApplyForce(airForce);
            // Try to 0 out movement and only move in target direction
            if (_moveDir.LengthSquared() > 0.01) {
                var vel0 = state.LinearVelocity.WithY(0);
                var dot = 0.5f - _head.Transform.Basis.Z.Normalized().Dot(_moveDir2.Normalized()) / 2f;
                var t = _moveDir2 * dot / 2f;
                var f = t - vel0;
                state.ApplyForce(f);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
    }

    // MOVEMENT
    private void Jump()
    {
        ApplyImpulse(Vector3.Up * JumpForce);
        IsJumping = true;
    }

    private void StartDash()
    {
        IsDashing = true;
        _wasDashing = true;
        _dashDir = _moveDir.LengthSquared() > 0.01f ? _moveDir : _head.Forward();
        _dashTimer = DashDuration;
    }

    private void StopDash()
    {
        IsDashing = false;
    }
}