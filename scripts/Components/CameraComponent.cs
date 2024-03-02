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
	[Export] public Node3D Follow;
	[Export] public float FollowSpeed;
	[Export] public Vector3 Offset;

    public override void _EnterTree()
	{
		if (_isActive && Game.ActiveCamera != this) {
            IsActive = true;
		}

		if (ShouldFollow) {
			TopLevel = true;
		}
	}

    public override void _Process(double delta)
	{
		if (ShouldFollow && Follow != null) {
			var targetPos = Follow.GlobalPosition;
			GlobalPosition = GlobalPosition.Lerp(targetPos, Game.DeltaTime * FollowSpeed);
			_cam.Position = Offset;
			Offset = Vector3.Zero;
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