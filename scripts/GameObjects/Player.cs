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
    [Export] private float MaxFallSpeed;
    [Export] private float JumpForce;
    [Export] private float JumpReleaseMult;

    [ExportSubgroup("Dash")]
    [Export] private float DashStaminaCost = 1f;
    [Export] private float DashDuration = 0.2f;
    [Export] private float DashSpeed;
    [Export] private float DashEndSpeed;

    [ExportSubgroup("Dash Jump")]
    [Export] private float DashJumpStaminaCost = 2f;
    [Export] private float DashJumpHopForce;
    [Export] private float DashJumpLeapForce;

    [ExportSubgroup("Slide")]
    [Export] private float SlideBaseSpeed;
    [Export] private float SlideDirChangeMult;
    [Export] private float SlideSpeedReduce;
    [Export] private float SlideLeapMultiplier;
    [Export] private float SlideHopForce;
    [Export] private float SlideMaxSpeedBuffer = 0.1f;

    [ExportSubgroup("Thwomp")]
    [Export] private float ThwompDownForce;
    [Export] private float ThwompJumpBuffer = 0.05f;
    [Export] private float ThwompForceTimeMult;
    // [Export] private float ThwompForce

    // States
    private bool CanJump => _groundCheck.IsOnGround && !IsJumping;
    public bool IsJumping { get; private set; } = false;
    
    public bool IsDashJump { get; private set; } = false;
    public bool IsDashing { get; private set; } = false;
   
    public bool IsSliding { get; private set; } = false;
  
    private bool CanTwhomp => !_groundCheck.IsOnGround && !IsSliding;
    public bool IsThwomping { get; private set; } = false;

    // Input
    private Vector3 _moveDir;
    private Vector3 _moveDir2;

    // Stamina
    private float _stamina;

    // Dash
    private Vector3 _dashDir;
    private float _dashTimer;

    // Slide
    private Vector3 _slideDir;
    private float _slideSpeed;

    private float _maxSlideSpeedBuffer;
    private float _maxSlideSpeedBufferTimer;

    // Thwomp
    private float _thwompForce;
    private float _thwompJumpBufferTimer;

    private bool _boost = false;
    private bool _falling = false;
    private bool _heavyFall = false;

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

        _groundCheck.OnIsGrounded += () => {
            if (IsThwomping) {
                EndThwomp();
            }

            IsJumping = false;
            
            _boost = false;
            _falling = false;
            _heavyFall = false;
        };

        // _groundCheck.OnIsGrounded += () => {
            // TODO(calco): ground thing
            // if (LinearVelocity.Y > 5f) {
                // var twen = CreateTween();
                // var start = Vector3.Zero;
                // var final = Vector3.Down * 0.5f;
                // twen.TweenProperty(_head, "position", final, 0.1f);
                // twen.Chain().TweenProperty(_head, "position", start, 0.1f);
                // twen.Play();
                // twen.Finished += () => twen.Free();
            // }
        // };
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
        if (!IsSliding && _groundCheck.IsOnGround && _input.Slide == KeyState.Pressed) {
            StartSlide();
        }
        if (IsSliding && _input.Slide == KeyState.Released) {
            EndSlide();
        }

        // Thwomp
        if (CanTwhomp && _input.Slide == KeyState.Pressed) {
            GroundThwomp();
        }
        _thwompJumpBufferTimer -= Game.DeltaTime;

        // Jump and all the other variations
        if (CanJump && _input.Jump == KeyState.Pressed) {
            if (_thwompJumpBufferTimer > 0f) {
                GD.Print("SUUUUUUUPA JUAMP!");
                SuperJump();
            }
            else if (IsDashing) {
                DashJump(_stamina < DashJumpStaminaCost);
                // TODO(calco): Maybe move this to dashjump() ???
                _stamina -= DashJumpStaminaCost;
            }
            else if (IsSliding) {
                SlideJump();
            } else {
                Jump();
            }
        }
        // TODO(calco): Variable jump

        if (IsSliding) {
            var sign = _slideSpeed < SlideBaseSpeed ? -1f : 1f;
            _slideSpeed -= sign * Game.DeltaTime * SlideSpeedReduce;

            // TODO(calco): Temporary, just showcasing slide
            var d = CameraManager.Instance.Rotation;
            d.X = Mathf.DegToRad(5 * Mathf.Sign(_input.Movement.X));
            CameraManager.Instance.Rotation = d;
        } else {
            _maxSlideSpeedBufferTimer -= Game.DeltaTime;
            if (_maxSlideSpeedBufferTimer < 0f) {
                _maxSlideSpeedBufferTimer = SlideMaxSpeedBuffer;
                // TODO(calco): Maybe scale this some way
                _maxSlideSpeedBuffer = LinearVelocity.Length();
            }
        }

        if (IsThwomping) {
            _thwompForce += Game.DeltaTime * ThwompForceTimeMult;
        }

        var right = _head.GlobalTransform.Basis.X * -_input.Movement.X;
        var forward = _head.GlobalTransform.Basis.Z * _input.Movement.Y;
        _moveDir = (forward + right).Normalized();

        _ui.DisplayStamina(_stamina);
    }

    private Vector3 _diffDir;
    private bool _diff;
    private bool _ifDashing;
    private bool _ifDashJump;
    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        _moveDir2 = _moveDir * Game.FixedDeltaTime * RunSpeed;

        if (IsSliding) {
            GravityScale = _groundCheck.IsOnGround ? 1f : 0f;
            var slideDir2 = (_slideDir + _moveDir * SlideDirChangeMult).Normalized();
            state.LinearVelocity = slideDir2 * _slideSpeed;
            return;
        }

        if (IsDashing) {
            GravityScale = 0f;
            state.LinearVelocity = _dashDir * DashSpeed;
        } 
        else if (_ifDashing) {
            _ifDashing = false;
            GravityScale = 1f;

            if (!IsDashJump || !_ifDashJump) {
                _ifDashJump = false; // redundant?
                state.LinearVelocity = _dashDir * DashEndSpeed;
            } else {
                _ifDashJump = false;
            }
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
            GravityScale = _heavyFall ? 2f : 1f;

            // Limit velocity
            var maxFall = MaxFallSpeed * GravityScale;
            if (state.LinearVelocity.Y < -maxFall) {
                state.LinearVelocity = state.LinearVelocity.WithY(-maxFall);
            }

            // TODO(calco): Maybe don't do this lol
            if (IsThwomping) {
                state.LinearVelocity = Vector3.Down * ThwompDownForce;
                return;
            }

            var vel = state.LinearVelocity;

            var cX = (_moveDir2.X > 0 && vel.X < _moveDir2.X) || (_moveDir2.X < 0 && vel.X > _moveDir2.X);
            var cZ = (_moveDir2.Z > 0 && vel.Z < _moveDir2.Z) || (_moveDir2.Z < 0 && vel.Z > _moveDir2.Z);

            var airForce = new Vector3(cX ? _moveDir2.X : 0, _moveDir2.Y, cZ ? _moveDir2.Z : 0) * AirAccelMult;

            state.ApplyForce(airForce);
            // Try to 0 out movement and only move in target direction
            var vel0 = state.LinearVelocity.WithY(0);
            if (_moveDir.LengthSquared() > 0.01 && vel0.Normalized().DotLess(_moveDir, 0.99f, false)) {
                var v = Mathf.Abs(_head.Transform.Basis.Z.Normalized().Dot(_moveDir2.Normalized()));;
                var t = Mathf.Max(RunSpeed * Game.FixedDeltaTime, vel0.Length()) * _moveDir;
                var f = t - vel0;
                var mult = (IsJumping && IsDashJump ? 2f : 1f) * (1f + 0.25f * v) * AirAccelMult;
                if (_boost) {
                    mult *= 0.25f;
                }
                state.ApplyForce(f * mult);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
    }

    // MOVEMENT
    private void Jump(float force = -1f)
    {
        IsJumping = true;
        IsDashJump = false;
        _falling = true;
        _heavyFall = false;
        _boost = false;
        // TODO(calco): Set other default state, isslidejump?
        ApplyImpulse(Vector3.Up * (force == -1f ? JumpForce : force));
    }

    private void StartDash()
    {
        IsThwomping = false;
        _heavyFall = false;
        _falling = false;
        _boost = true;
        IsDashing = true;
        IsJumping = false;
        _ifDashing = true;
        _dashDir = _moveDir.LengthSquared() > 0.01f ? _moveDir : _head.Forward();
        _dashTimer = DashDuration;
    }

    private void StopDash()
    {
        IsDashing = false;
        _boost = false;
        
        _falling = true;
    }

    private void DashJump(bool halfJump)
    {
        Jump(DashJumpHopForce);
        IsDashing = false;
        IsJumping = true;
        IsDashJump = true;
        _boost = true;
        
        _ifDashJump = true;
        ApplyImpulse(_dashDir * DashJumpLeapForce * (halfJump ? 0.5f : 1f));
    }

    private void StartSlide()
    {
        IsSliding = true;
        _slideDir = _moveDir.LengthSquared() > 0.01f ? _moveDir : _head.Forward();
        _slideSpeed = Mathf.Max(LinearVelocity.Length(), SlideBaseSpeed);
        _slideSpeed = Mathf.Max(_slideSpeed, _maxSlideSpeedBuffer);
        _maxSlideSpeedBufferTimer = 0f;

        // TODO(calco): Temporary, just showcasing slide
        _head.Position += Vector3.Down;
    }

    private void EndSlide()
    {
        _maxSlideSpeedBufferTimer = 0f;

        // TODO(calco): Temporary, just showcasing slide
        _head.Position += Vector3.Up;
        var d = CameraManager.Instance.RotationDegrees;
        d.X = 0;
        CameraManager.Instance.RotationDegrees = d;
        IsSliding = false;
    }

    private void SlideJump()
    {
        EndSlide();
        Jump(SlideHopForce);
        _boost = true;
        
        // GD.Print("V: ", LinearVelocity.Length());
        var l = LinearVelocity.Length();
        var mult = 1f;
        // TODO(calco): MATH FORMULA THIS LMAO
        if (l > 15) mult = 0.95f;
        else if (l > 20) mult = 0.85f;
        else if (l > 25) mult = 0.7f;
        else if (l > 40) mult = 0.5f;
        else if (l > 50) mult = 0.25f;
        else if (l > 55) mult = 0.1f;
    
        var f = Mathf.Clamp(_slideSpeed * SlideLeapMultiplier, 0f, 20f) * mult;
        // GD.Print("F: ", f);
        ApplyImpulse(_slideDir * f);
    }

    private void GroundThwomp()
    {
        IsThwomping = true;
        _falling = true;
        _heavyFall = true;
        _thwompForce = 0;
    }

    private void EndThwomp()
    {
        IsThwomping = false;
        _thwompJumpBufferTimer = ThwompJumpBuffer;

        // TODO(calco): More visuals, just like the rest of "end"...

        if (_input.Slide == KeyState.Down) {
            // TODO(calco): Add a sort of slam
        }
    }

    private void SuperJump()
    {
        GD.Print("SUPER JUMP: ", _thwompForce);
        Jump(JumpForce * 1.5f + _thwompForce * 4f);
        _thwompForce = 0f;
    }
}