using Godot;

namespace Yellow.Resources;

[GlobalClass]
public partial class PlayerMovementSO : Resource
{
    [ExportGroup("Movement")]
	[Export] public float MaxStamina = 3f;

	[ExportSubgroup("Run")]
	[Export] public float RunSpeed = 600f;
	[Export] public float AirAccelMult = 1.5f;

	[ExportSubgroup("Jump")]
	[Export] public float MaxFallSpeed = 40f;
	[Export] public float JumpForce = 17.75f;

	[ExportSubgroup("Wall Jump")]
	[Export] public float WallSlideMaxDot = -0.85f;
	[Export] public float WallSlideGravityScale = 0.2f;
	[Export] public float WallSlideMaxFallSpeed = 20f;
	[Export] public float WallJumpHopForce = 16f;
	[Export] public float WallJumpLeapForce = 17f;
	[Export] public int WallJumpMaxCount = 3;

	[ExportSubgroup("Dash")]
	[Export] public float DashStaminaCost = 1f;
	[Export] public float DashDuration = 0.175f;
	[Export] public float DashSpeed = 35f;
	[Export] public float DashEndSpeed = 10f;

	[ExportSubgroup("Dash Jump")]
	[Export] public float DashJumpStaminaCost = 1f;
	[Export] public float DashJumpHopForce = 13f;
	[Export] public float DashJumpLeapForce = 5f;

	[ExportSubgroup("Slide")]
	[Export] public float SlideBaseSpeed = 13.5f;
	[Export] public float SlideDirChangeMult = 0.2f;
	[Export] public float SlideSpeedReduce = 1f;
	[Export] public float SlideLeapMultiplier = 0.55f;
	[Export] public float SlideHopForce = 16f;
	[Export] public float SlideMaxSpeedBuffer = 0.1f;

	[ExportSubgroup("Thwomp")]
	[Export] public float ThwompDownForce = 50f;
	[Export] public float ThwompJumpBuffer = 0.1f;
	[Export] public float ThwompForceTimeMult = 8f;
}