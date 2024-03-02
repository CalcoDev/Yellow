using Godot;
using Yellow.Managers;

namespace Yellow.Components;

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
		_cam.RotateX(-y);
		
		const float MIN_ROT = -89.99f * (Mathf.Pi / 180f);
		const float MAX_ROT = 89.99f * (Mathf.Pi / 180f);
		_cam.Rotation = new (
			Mathf.Clamp(_cam.Rotation.X, MIN_ROT, MAX_ROT),
			_cam.Rotation.Y,
			_cam.Rotation.Z
		);
	}
}