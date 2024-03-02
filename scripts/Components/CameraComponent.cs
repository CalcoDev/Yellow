using Godot;
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
	}

    // NOTE(calco): Rotation helpers
	public void MouseRotation(float x, float y)
	{
		RotateY(x);

		const float MAX_ADD = 0.1f;
		if (
			(_cam.RotationDegrees.X < 0f && _cam.RotationDegrees.X < -88f && y > 0f) ||
			(_cam.RotationDegrees.X > 0f && _cam.RotationDegrees.X > 88f && y < 0f)
		) {
			int addCnt = Mathf.CeilToInt(y / MAX_ADD);
			for (int i = 0; i < addCnt; ++i) {
				var add = Mathf.Min(MAX_ADD * Mathf.Sign(y), y);
				y -= add;
				_cam.RotateX(-y);
				if (ClampRot()) {
					break;
				}
			}
		} else {
			_cam.RotateX(-y);
		}
	}

	private bool ClampRot()
	{
		const float MIN_ROT = -88.99f * (Mathf.Pi / 180f);
		const float MAX_ROT = 88.99f * (Mathf.Pi / 180f);
		var prev = _cam.Rotation.X;
		_cam.Rotation = new (
			Mathf.Clamp(_cam.Rotation.X, MIN_ROT, MAX_ROT),
			0f,
			0f
		);
		return prev == _cam.Rotation.X;
	}
}