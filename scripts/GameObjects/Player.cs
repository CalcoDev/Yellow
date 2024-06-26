using System;
using Godot;
using Yellow.Components;
using Yellow.Extensions;
using Yellow.Managers;
using Yellow.Misc;
using Yellow.Nodes;
using Yellow.Resources;

namespace Yellow.GameObjects;

using static Yellow.Resources.PlayerInput;

[GlobalClass]
public partial class Player : RigidBody3D
{
	public static Player Instance { get; private set; }

	[ExportGroup("References")]
	[Export] private PlayerInput _input;
	[Export] private GroundCheckComponent _groundCheck;
	[Export] private ShapeCastComponent _slopeCheck;
	[Export] private ShapeCastComponent _wallCheck;
	[Export] private CameraComponent _playerCamera;

	[Export] private HealthComponent _health;

	[ExportGroup("SFX")]
	[Export] private float SFXWalkStepDist = 1f;
	
	[ExportSubgroup("VFX")]
	[Export] private GpuParticles3D _vfxSpeedLines;
	[Export] private DistanceParticles _vfxDash;
	[Export] private GpuParticles3D _vfxSlide;
	[Export] private DistanceParticles _vfxSlideSmoke;
	[Export] private DistanceParticles _vfxSlideLines;
	[Export] private GpuParticles3D _vfxJump;
	[Export] private GpuParticles3D _vfxLand;

	[ExportGroup("Settings")]
	[Export] private float Sensitivity;
	[Export] private PlayerMovementSO _p;

	[Node("Head")] public Node3D Head { get; private set; }

	[Export] private Camera3D _weaponCamera;
	
	private Label _healthText;
	private ProgressBar _healthBar;
	private ProgressBar[] _staminaBars = new ProgressBar[3];

	// States
	public bool CanSlideWall => _wallCheck.IsColliding && _wallCheck.IsOnSlope;
	public bool IsWallSliding { get; private set; } = false;

	private bool CanJump => _groundCheck.IsOnGround && !IsJumping;
	private bool CanWallJump => !_groundCheck.IsOnGround && CanSlideWall && _wallJumpCount > 0;
	public bool IsJumping { get; private set; } = false;
	
	public bool IsDashJump { get; private set; } = false;
	public bool IsDashing { get; private set; } = false;
   
	public bool IsSliding { get; private set; } = false;
  
	private bool CanTwhomp => !_groundCheck.IsOnGround && !IsSliding;
	public bool IsThwomping { get; private set; } = false;

	// Input
	private Vector3 _moveDir = Vector3.Zero;
	private Vector3 _moveDir2 = Vector3.Zero;

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

	// Wall Jump
	private int _wallJumpCount;

	private bool _boost = false;
	private bool _falling = false;
	private bool _heavyFall = false;

	// SFX
	private float _walkDistCheck = 0f;

	private Vector3 _prevPos;

	public override void _Notification(int what)
	{
		if (what == NotificationSceneInstantiated || what == NotificationEnterTree) {
			this.WireNodes();
		}
	}

    public override void _EnterTree()
    {
        if (Instance != null) {
			GD.PushWarning("WARN: Player instance already exists!");
			QueueFree();
			return;
		}
		Instance = this;
    }

    public override void _ExitTree()
    {
        Instance = null;
    }

	public void DisplayStamina(float stamina)
	{
		for (int i = 0; i < 3; ++i) {
			float s = Mathf.Clamp(stamina - i, 0, 1f);
			_staminaBars[i].Value = s;
		}
	}

    public override void _Ready()
	{

		ProcessPriority = (int)NodeProcessOrder.Player;
		AddToGroup("player");

		_healthText = GetNode<Label>("%HealthText");
		_healthBar = GetNode<ProgressBar>("%Health");
		_staminaBars[0] = GetNode<ProgressBar>("%Slide1");
		_staminaBars[1] = GetNode<ProgressBar>("%Slide2");
		_staminaBars[2] = GetNode<ProgressBar>("%Slide3");
		
		_health.OnHealthChanged += OnHealthChanged;
		_healthBar.Value = _health.GetHealthPercentage();
		_healthText.Text = Mathf.FloorToInt(_health.GetHealthPercentage() * 100f).ToString();

		foreach (var bar in _staminaBars) {
			bar.MinValue = 0f;
			bar.MaxValue = 1f;
			bar.Value = 0;
		}

		_groundCheck.OnIsGrounded += () => {
			if (!IsSliding) {
				if (IsThwomping || _heavyFall) {
					SoundManager.Instance.Play("player_land_heavy");
					_playerCamera.ShakeLength(10, 1, 0.2f, true);
					Game.Hitstop(0.05f);
				} else {
					SoundManager.Instance.Play("player_land");
				}
			}
			
			if (IsThwomping) {
				EndThwomp();
			}
			IsJumping = false;

			_boost = false;
			_falling = false;
			_heavyFall = false;
			_wallJumpCount = _p.WallJumpMaxCount;

			var tween = CreateTween();
			var offset = _p.CameraLandOffset * Mathf.Clamp(LinearVelocity.Y / 10f, 1f, 4f);
			var td = 0.1f * Mathf.Clamp(LinearVelocity.Y / 20f, 1f, 2f) * 0.5f;
			tween.TweenProperty(_playerCamera, "HardOffset", Vector3.Down * offset, td);
			tween.Chain().TweenProperty(_playerCamera, "HardOffset", Vector3.Zero, td * 1.5f);
			tween.Play();
		};
	}

    private void OnHealthChanged(float previous, float current, float maximum)
    {
		_healthBar.Value = _health.GetHealthPercentage();
		_healthText.Text = Mathf.FloorToInt(_health.GetHealthPercentage() * 100f).ToString();
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
			HandleCameraRotation(x, y);
		}
	}

	private bool _didMoveWallSlide = false;
	private bool _prevDidMoveWallSlide = false;
	public override void _Process(double delta)
	{
		_weaponCamera.GlobalTransform = _playerCamera.Cam.GlobalTransform;
		
		// NOTE(drts): controller
		// TODO(calco): Should do a check for if controller active or not
		float lookHoriz = Input.GetAxis("look_left", "look_right")/Sensitivity;
		float lookVert = Input.GetAxis("look_up", "look_down")/Sensitivity;
		HandleCameraRotation(-lookHoriz, lookVert);

		// NOTE(calco): invert because I think of it as LHS not RHS
		var right = Head.RightXZ() * -_input.Movement.X;
		var forward = Head.ForwardXZ() * _input.Movement.Y;
		_moveDir = (forward + right).Normalized();

		// Tilt camera
		if (!IsSliding) {
			// TODO(calco): Causes camera to go haywire sometimes
			var tz = _input.Movement.X * _p.CameraSideTiltAngle;
			var cz = Mathf.Lerp(_playerCamera.Cam.RotationDegrees.Z, tz, Game.DeltaTime * 20f);
			_playerCamera.Cam.RotationDegrees = _playerCamera.Cam.RotationDegrees.WithZ(cz);
		}

  		if (!IsSliding) {
			_stamina = Mathf.Clamp(_stamina + Game.DeltaTime, 0f, _p.MaxStamina);
		}

		_didMoveWallSlide = _moveDir.Length() > 0.01f;
		if (CanSlideWall) {
			if (_wallCheck.ClosestNormal.DotLess(_moveDir, _p.WallSlideMaxDot, false)) {
				IsWallSliding = true;
				if (_didMoveWallSlide && !_prevDidMoveWallSlide) {
					_ifWallSliding = false;
				}
			} else {
				IsWallSliding = false;
			}
		} else {
			IsWallSliding = false;
		}
		_prevDidMoveWallSlide = _didMoveWallSlide;

		// Dash
		if (IsDashing) {
			_dashTimer -= Game.DeltaTime;
			if (_dashTimer < 0f) {
				StopDash();
			}
		} else if (_input.Dash == KeyState.Pressed && _stamina > _p.DashStaminaCost) {
			_stamina -= _p.DashStaminaCost;
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
				SuperJump();
			} else if (IsDashing) {
				DashJump(_stamina < _p.DashJumpStaminaCost);
				// TODO(calco): Maybe move this to dashjump() ???
				_stamina -= _p.DashJumpStaminaCost;
			} else if (IsSliding) {
				SlideJump();
			} else {
				Jump();
			}
		}
		
		if (CanWallJump && _input.Jump == KeyState.Pressed) {
			WallJump();
		}

		// TODO(calco): Variable jump

		if (IsSliding) {
			var sign = _slideSpeed < _p.SlideBaseSpeed ? -1f : 1f;
			_slideSpeed -= sign * Game.DeltaTime * _p.SlideSpeedReduce;

			// TODO(calco): Temporary, just showcasing slide
			var d = _playerCamera.Rotation;
			d.Z = Mathf.DegToRad(5 * Mathf.Sign(_input.Movement.X));
			_playerCamera.Rotation = d;
		} else {
			_maxSlideSpeedBufferTimer -= Game.DeltaTime;
			if (_maxSlideSpeedBufferTimer < 0f) {
				_maxSlideSpeedBufferTimer = _p.SlideMaxSpeedBuffer;
				// TODO(calco): Maybe scale this some way
				_maxSlideSpeedBuffer = LinearVelocity.Length();
			}
		}

		if (IsThwomping) {
			_thwompForce += Game.DeltaTime * _p.ThwompForceTimeMult;
		}

		// NOTE(calco): Visual Effects
		// TODO(calco): Maybe don't zero out y velocity?
		var speed = LinearVelocity.WithY(0).Length();
		var tAmount = Mathf.Clamp((speed - 8f) * 5f, 0, 250);
		var amount = Mathf.Lerp(_vfxSpeedLines.AmountRatio, tAmount, Game.DeltaTime * 5f);
		if (speed < 5f || (amount == 0 && _vfxSpeedLines.Emitting)) {
			_vfxSpeedLines.Emitting = false;
		} else if (amount >= 1 && !_vfxSpeedLines.Emitting) {
			_vfxSpeedLines.Emitting = true;
		}
		_vfxSpeedLines.AmountRatio = amount / 20f;
		
		var tRing = Mathf.Clamp(speed * 0.1f, 1.25f, 2f);
		var cRing = _vfxSpeedLines.ProcessMaterial.Get("emission_ring_inner_radius").As<float>();
		_vfxSpeedLines.ProcessMaterial.Set("emission_ring_inner_radius", Mathf.Lerp(cRing, tRing, Game.DeltaTime * 2f));
		
		var baseVelocity = Mathf.Clamp(speed * 0.25f, 5f, 15f);
		var rng = GD.RandRange(0, 0.2f);
		_vfxSpeedLines.ProcessMaterial.Set("initial_velocity_min", baseVelocity * (1f - rng));
		_vfxSpeedLines.ProcessMaterial.Set("initial_velocity_max", baseVelocity * (1f + rng));

		// dash
		// _vfxDash.Rotation = Vector3.Zero;

		// SFX
		_walkDistCheck += _prevPos.WithY(0).DistanceTo(GlobalPosition.WithY(0));
		if (_walkDistCheck > SFXWalkStepDist) {
			_walkDistCheck = 0;
			if (_groundCheck.IsOnGround && !IsSliding && !IsDashing) {
				SoundManager.Instance.Play("player_walk");
			}
		}

		if ((IsSliding && _groundCheck.IsOnGround) || (!_groundCheck.IsOnGround && LinearVelocity.Y >= 0f && IsWallSliding)) {
			SoundManager.Instance.Play("player_slide", true);
		} else if (SoundManager.Instance.IsAudioPlaying("player_slide")) {
			SoundManager.Instance.StopAllName("player_slide", true);
		}

		_prevPos = GlobalPosition;

		DisplayStamina(_stamina);
	}

	private Vector3 _diffDir;
	private bool _diff;
	private bool _ifDashing;
	private bool _ifDashJump;
	private bool _ifWallSliding = false;
	private bool _ifWasGrounded = false;
	private float _ifLastGroundSlope = 0f;
	private Vector3 _ifPrevMovePlatformOffset = Vector3.Zero;
	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		_moveDir2 = _moveDir * Game.FixedDeltaTime * _p.RunSpeed;
		if (_slopeCheck.IsOnSlope && !IsJumping) {
			var old = _moveDir2;
			_moveDir = _moveDir.Slide(_slopeCheck.ClosestNormal).Normalized();
			_moveDir2 = _moveDir * Game.FixedDeltaTime * _p.RunSpeed;
			// TODO(calco): This doesn't make sense but kinda works.
			_moveDir2 = _moveDir2.WithX(old.X).WithZ(old.Z);
		}

		if (IsSliding) {
			GravityScale = _groundCheck.IsOnGround ? 0f : 1f;
			var slideDir2 = (_slideDir + _moveDir * _p.SlideDirChangeMult).Normalized();
			var y = state.LinearVelocity.Y;
			state.LinearVelocity = (slideDir2 * _slideSpeed).WithY(y);
			return;
		}

		if (IsDashing) {
			GravityScale = 0f;
			state.LinearVelocity = _dashDir * _p.DashSpeed;
		} 
		else if (_ifDashing) {
			_ifDashing = false;
			GravityScale = 1f;

			if (!IsDashJump || !_ifDashJump) {
				_ifDashJump = false; // redundant?
				state.LinearVelocity = _dashDir * _p.DashEndSpeed;
			} else {
				_ifDashJump = false;
			}
		}

		var setThing = false;
		if (IsWallSliding && !_ifWallSliding && state.LinearVelocity.Y < 0f) {
			// !!! TODO(calco): FIX THIS LOL
			var y = Mathf.Min(state.LinearVelocity.Y * 0.2f, 0.5f);
			state.LinearVelocity = state.LinearVelocity.WithY(y);
			setThing = true;
		}
		if (setThing) {
			_ifWallSliding = IsWallSliding;
		}

		if (_groundCheck.IsOnGround && !IsJumping) {
			GravityScale = _moveDir.LengthSquared() < 0.01f ? 0 : 1;

			float t = 0.25f * (1f + _groundCheck.GroundProperties.Friction);
			state.LinearVelocity = state.LinearVelocity.Lerp(_moveDir2, t);
			
			state.SetConstantForce(Vector3.Zero);
			
			// TODO(calco): Multiply this by sth.
			if (_slopeCheck.IsOnSlope) {
				state.LinearVelocity += -_slopeCheck.ClosestNormal;
			}

			_diff = false;
		}
		else {
			GravityScale = _heavyFall ? 2f : 1f;
			
			// Limit velocity
			var maxFall = _p.MaxFallSpeed * GravityScale;

			if (IsWallSliding && state.LinearVelocity.Y < 0f) {
				GravityScale = _p.WallSlideGravityScale;
				maxFall = _p.WallSlideMaxFallSpeed;
			}

			if (state.LinearVelocity.Y < -maxFall) {
				state.LinearVelocity = state.LinearVelocity.WithY(-maxFall);
			}

			// TODO(calco): Maybe don't do this lol
			if (IsThwomping) {
				state.LinearVelocity = Vector3.Down * _p.ThwompDownForce;
				return;
			}

			var vel = state.LinearVelocity;

			var cX = (_moveDir2.X > 0 && vel.X < _moveDir2.X) || (_moveDir2.X < 0 && vel.X > _moveDir2.X);
			var cZ = (_moveDir2.Z > 0 && vel.Z < _moveDir2.Z) || (_moveDir2.Z < 0 && vel.Z > _moveDir2.Z);

			var airForce = new Vector3(cX ? _moveDir2.X : 0, _moveDir2.Y, cZ ? _moveDir2.Z : 0) * _p.AirAccelMult;

			state.ApplyForce(airForce);
			// Try to 0 out movement and only move in target direction
			var vel0 = state.LinearVelocity.WithY(0);
			if (_moveDir.LengthSquared() > 0.01 && vel0.Normalized().DotLess(_moveDir, 0.99f, false)) {
				var v = Mathf.Abs(Head.Transform.Basis.Z.Normalized().Dot(_moveDir2.Normalized()));;
				var t = Mathf.Max(_p.RunSpeed * Game.FixedDeltaTime, vel0.Length()) * _moveDir;
				var f = t - vel0;
				var mult = (IsJumping && IsDashJump ? 2f : 1f) * (1f + 0.25f * v) * _p.AirAccelMult;
				if (_boost) {
					mult *= 0.25f;
				}
				state.ApplyForce(f * mult);
			}
		}

		_ifWasGrounded = _groundCheck.IsOnGround;
		if (_groundCheck.IsOnGround && _slopeCheck.WasColliding) {
			_ifLastGroundSlope = _slopeCheck.SlopeAngle;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_groundCheck.IsOnMovingPlatform) {
			GlobalPosition += _groundCheck.MovingPlatformVelocity * Game.FixedDeltaTime;
		}
	}

	// MOVEMENT
	private void Jump(float force = -1f, bool playAudio = true)
	{
		IsJumping = true;
		IsDashJump = false;
		_falling = true;
		_heavyFall = false;
		_boost = false;
		// TODO(calco): Set other default state, isslidejump?
		LinearVelocity = LinearVelocity.WithY(0);
		ApplyImpulse(Vector3.Up * (force == -1f ? _p.JumpForce : force));

		if (playAudio) {
			SoundManager.Instance.Play("player_jump");
		}
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
		_dashDir = _moveDir.LengthSquared() > 0.01f ? _moveDir : Head.ForwardXZ();
		_dashTimer = _p.DashDuration;

		_playerCamera.Cam.Fov += _p.CameraDashFovMod;

		var dir = GetHeadOffsetBasedOnInputDir(_input.Movement);
		_vfxDash.GlobalPosition = Head.GlobalPosition - dir * 2f;
		_vfxDash.LookAt(Head.GlobalPosition, Vector3.Up);
		_vfxDash.GlobalPosition = Head.GlobalPosition;
		_vfxDash.Rotate(-dir.Cross(Vector3.Up).Normalized(), Mathf.Pi / 2f);
		_vfxDash.Emitting = true;
		_vfxDash.FreezeNextFrame = true;
		((CylinderMesh)_vfxDash.DrawPass1).Height = _p.DashDuration * _p.DashSpeed * 10f;
		
		SoundManager.Instance.Play("player_dash");
	}

	private Vector3 GetHeadOffsetBasedOnInputDir(Vector2 dir)
	{
		var x = -Head.RightXZ() * Mathf.Sign(dir.X);
		var z = Head.ForwardXZ() * Mathf.Sign(dir.Y);
		var f = (x + z).Normalized();
		return f == Vector3.Zero ? Head.ForwardXZ() : f;
	}

	private void StopDash()
	{
		IsDashing = false;
		_boost = false;
		
		_falling = true;
		_playerCamera.Cam.Fov -= _p.CameraDashFovMod;
		_vfxDash.Emitting = false;

		_vfxDash.Emitting = false;
	}

	private void DashJump(bool halfJump)
	{
		Jump(_p.DashJumpHopForce, false);
		_playerCamera.Cam.Fov -= _p.CameraDashFovMod;
		IsDashing = false;
		IsJumping = true;
		IsDashJump = true;
		_boost = true;
		
		_ifDashJump = true;
		ApplyImpulse(_dashDir * _p.DashJumpLeapForce * (halfJump ? 0.5f : 1f));
		
		SoundManager.Instance.Play("player_dash_jump");
	}

	private void StartSlide()
	{
		IsSliding = true;
		_slideDir = _moveDir.LengthSquared() > 0.01f ? _moveDir : Head.ForwardXZ();
		// TODO(calco): With Y 0???
		_slideSpeed = Mathf.Max(LinearVelocity.WithY(0).Length(), _p.SlideBaseSpeed);
		_slideSpeed = Mathf.Max(_slideSpeed, _maxSlideSpeedBuffer);
		_maxSlideSpeedBufferTimer = 0f;

		// TODO(calco): Temporary, just showcasing slide
		Head.Position += Vector3.Down * _p.CameraSlideDownMod;
		
		_playerCamera.Cam.Fov += _p.CameraSlideFovMod;

		var dir = GetHeadOffsetBasedOnInputDir(_input.Movement);
		_vfxSlide.GlobalPosition = Head.GlobalPosition - dir * 2f;
		_vfxSlide.LookAt(Head.GlobalPosition, Vector3.Up);
		_vfxSlide.GlobalPosition = Head.GlobalPosition + dir * 0.65f + Vector3.Down * 0.45f;
		_vfxSlide.Rotate(-dir.Cross(Vector3.Up).Normalized(), Mathf.Pi / 2f);
		var vel = 5f;
		_vfxSlide.ProcessMaterial.Set("initial_velocity_min", vel);
		_vfxSlide.ProcessMaterial.Set("initial_velocity_max", vel);

		_vfxSlide.Emitting = true;

		_vfxSlideSmoke.GlobalPosition = Head.GlobalPosition - dir * 0.75f + Vector3.Down * 0.55f;
		_vfxSlideSmoke.Emitting = true;
		_vfxSlideSmoke.FreezeNextFrame = true;

		_vfxSlideLines.GlobalPosition = Head.GlobalPosition - dir * 2f;
		_vfxSlideLines.LookAt(Head.GlobalPosition, Vector3.Up);
		_vfxSlideLines.GlobalPosition = Head.GlobalPosition;
		_vfxSlideLines.Rotate(-dir.Cross(Vector3.Up).Normalized(), Mathf.Pi / 2f);
		_vfxSlideLines.Emitting = true;
		_vfxSlideLines.FreezeNextFrame = true;

		((CapsuleShape3D)this.GetFirstNodeOfType<CollisionShape3D>().Shape).Height = 1f;
	}

	private void EndSlide()
	{
		((CapsuleShape3D)this.GetFirstNodeOfType<CollisionShape3D>().Shape).Height = 2f;

		_maxSlideSpeedBufferTimer = 0f;

		// TODO(calco): Temporary, just showcasing slide
		Head.Position += Vector3.Up * _p.CameraSlideDownMod;
		
		var d = _playerCamera.RotationDegrees;
		d.Z = 0;
		_playerCamera.RotationDegrees = d;
		IsSliding = false;

		_playerCamera.Cam.Fov -= _p.CameraSlideFovMod;
		_vfxSlide.Emitting = false;
		_vfxSlideSmoke.Emitting = false;
		_vfxSlideLines.Emitting = false;

		SoundManager.Instance.Play("player_slide_end");
	}

	private void SlideJump()
	{
		EndSlide();
		Jump(_p.SlideHopForce);
		_boost = true;
		
		// TODO(calco): Figure out length mult
		var l = LinearVelocity.WithY(0).Length();
		var mult = 1f;
	
		var f = Mathf.Clamp(_slideSpeed * _p.SlideLeapMultiplier, 0f, 20f) * mult;
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
		_thwompJumpBufferTimer = _p.ThwompJumpBuffer;

		// TODO(calco): More visuals, just like the rest of "end"...

		if (_input.Slide == KeyState.Down) {
			// TODO(calco): Add a sort of slam
		}
	}

	private void SuperJump()
	{
		Jump(_p.JumpForce * 1.25f + _thwompForce * 4f);
		_thwompForce = 0f;
	}

	private void WallJump()
	{
		_wallJumpCount -= 1;
		GravityScale = 1f;
		LinearVelocity = LinearVelocity.WithX(0).WithY(0);
		Jump(_p.WallJumpHopForce);
		ApplyImpulse(_wallCheck.ClosestNormal * _p.WallJumpLeapForce);
	}

	private void HandleCameraRotation(float x, float y)
	{
		_playerCamera.MouseRotation(x, y);
		Head.Rotation = _playerCamera.Rotation;
	}
}
