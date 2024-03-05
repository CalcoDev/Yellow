using System.Collections.Generic;
using Godot;
using Yellow.Extensions;
using Yellow.Managers;

namespace Yellow.Components;

[GlobalClass]
public partial class CameraComponent : Node3D
{
    [ExportGroup("References")]
    [Export] private Camera3D _cam;
	public Camera3D Cam => _cam;

    [ExportGroup("Settings")]
    [Export] public bool IsActive {
        get => _isActive;
        set {
            _isActive = value;
            // TODO(calco): I doubt we should set the active camera to null lmao
            Game.SetActiveCamera(_isActive ? this : null);
        }
    }
    private bool _isActive = false;

    [ExportSubgroup("Follow")]
	[Export] public bool ShouldFollow;
	
	// NOTE(calco): Godot is very very very dumb
	[Export] private bool FollowParent;
	
	[Export(PropertyHint.NodePathValidTypes, "Node3D")] public NodePath Follow;
	private Node3D _follow;

	[Export] public float FollowSpeed;
	[Export] public Vector3 HardOffset;
	[Export] public Vector3 Offset;

	[ExportSubgroup("Shake")]
	[Export] public bool CanShake = true;

	private abstract class CameraShake
	{
		public float Strength;
		public float Speed;

		public abstract bool Update(float dt);
		
		// TODO(calco): This should be a thing.
		// public abstract Vector3 GetShake();
	}

	private class ReductionCameraShake : CameraShake
	{
		public float ReductionRate;
		
		public override bool Update(float dt)
		{
			Strength -= ReductionRate;
			return Strength > 0;
		}

        public ReductionCameraShake(float strength, float speed, float reductionRate)
		{
			Strength = strength;
			Speed = speed;
			ReductionRate = reductionRate;
		}
	}

	private class LengthCameraShake : CameraShake
	{
		public float MaxLength;
		public float MaxStrength;
		public bool ReduceOverTime;
		
		public float LengthRemaining;

        public override bool Update(float dt)
        {
            LengthRemaining -= dt;
			if (ReduceOverTime) {
				// TODO(calco): Implement different attenuation models.
				Strength = GetShakeProgression() * MaxStrength;
			}
			return LengthRemaining > 0f;
        }

        public LengthCameraShake(float strength, float speed, float length, bool reduceOverTime)
		{
			Strength = strength;
			MaxStrength = strength;
			Speed = speed;
			MaxLength = length;
			LengthRemaining = length;
			ReduceOverTime = reduceOverTime;
		}

		public float GetShakeProgression()
		{
			return LengthRemaining / MaxLength;
		}
	}
	public bool IsShaking => _shakes.Count > 0;
	private readonly List<CameraShake> _shakes = new();
	private readonly FastNoiseLite _noise = new();

    public override void _EnterTree()
	{
		if (_isActive && Game.ActiveCamera != this) {
            IsActive = true;
		}

		if (ShouldFollow) {
			if (Follow != null) {
				_follow = GetNode<Node3D>(Follow);
			}
			if (FollowParent) {
				_follow = GetParent<Node3D>();
			}
			TopLevel = true;
		}
	}

	private Vector3 _prevOffset;
	private Vector3 _prevShakeAngles;
    public override void _Process(double delta)
	{
		if (ShouldFollow && _follow != null) {
			var targetPos = _follow.GlobalPosition;
			GlobalPosition = GlobalPosition.Lerp(targetPos, Game.DeltaTime * FollowSpeed);
		
			GlobalPosition = GlobalPosition - _prevOffset + Offset;
			// _prevOffset = Offset;
			Offset = Vector3.Zero;
			
			_cam.Position = HardOffset;
			HardOffset = Vector3.Zero;
		}

		if (CanShake) {
			_prevShakeAngles = Vector3.Zero;
			var currShakeAngles = Vector3.Zero;
			
			var toRemove = new HashSet<CameraShake>();
			foreach (var s in _shakes) {
				if (!s.Update(Game.DeltaTime)) {
					toRemove.Add(s);
				}

				// TODO(calco): Define a better way to do this.
				var str = s.Strength;
				var f = Game.Time * s.Speed * 100f;
				var shakeAngles = str * new Vector3(
					GetNoiseFromSeed(0, f),
					GetNoiseFromSeed(1, f),
					GetNoiseFromSeed(2, f)
				);
				currShakeAngles += shakeAngles;
			}
			foreach (var s in toRemove) {
				_shakes.Remove(s);
			}

			var r = _cam.RotationDegrees;
			r.X = currShakeAngles.X;
			r.Y = 180f + currShakeAngles.Y;
			r.Z = currShakeAngles.Z;
			_cam.RotationDegrees = r;
		}
	}

	// NOTE(calco): Shake helpers
	public void ShakeStrength(float strength, float speed, float reductionRate)
	{
		_shakes.Add(new ReductionCameraShake(strength, speed, reductionRate));
	}

	public void ShakeLength(float strength, float speed, float length, bool reduceOverTime)
	{
		_shakes.Add(new LengthCameraShake(strength, speed, length, reduceOverTime));
	}

	public void StopShake()
	{
		_shakes.Clear();
	}

    // NOTE(calco): Rotation helpers
	public void MouseRotation(float x, float y)
	{
		var r = Rotation;
		r.Y += x;
		r.X += y;
		Rotation = r;
		ClampRot();
	}

	private bool ClampRot()
	{
		const float MIN_ROT = -88.99f * (Mathf.Pi / 180f);
		const float MAX_ROT = 88.99f * (Mathf.Pi / 180f);
		var prev = Rotation.X;
		Rotation = new (
			Mathf.Clamp(Rotation.X, MIN_ROT, MAX_ROT),
			Rotation.Y,
			Rotation.Z
		);
		return prev == Rotation.X;
	}

	private float GetNoiseFromSeed(int seed, float f)
	{
		_noise.Seed = seed;
		return _noise.GetNoise1D(f);
	}
}