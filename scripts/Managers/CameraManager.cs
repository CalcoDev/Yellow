using Godot;
using Yellow.Components;
using Yellow.Misc;

namespace Yellow.Managers;

public partial class CameraManager : Node3D
{
	public static CameraManager Instance { get; private set; }

	[ExportGroup("Settings")]
	[Export] public Node3D Follow;
	[Export] public float FollowSpeed;

	[Node("FirstPersonCamera")] private Camera3D _cam;

	public override void _Notification(int what)
	{
		if (what == NotificationSceneInstantiated || what == NotificationEnterTree) {
			this.WireNodes();
		}
	}

	public override void _EnterTree()
	{
		if (Instance != null) {
			GD.PushWarning("WARN: Camera Manager already exists.");
			QueueFree();
			return;
		}

		Instance = this;
	}

	public override void _Process(double delta)
	{
		var targetPos = Follow.GlobalPosition;
		GlobalPosition = GlobalPosition.Lerp(targetPos, Game.DeltaTime * FollowSpeed);
	}

	public void DoRotation(float x, float y)
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
