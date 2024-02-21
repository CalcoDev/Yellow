using Godot;

namespace Yellow.Resources;

[GlobalClass]
public partial class GroundProperties : Resource
{
    [ExportGroup("Movement")]
    [ExportSubgroup("Run")]
    [Export] public float Friction = 1f;
    [Export] public float SpeedMultiplier = 1f;

    // TODO(calco): JUMP FORCE MULTIPLIER
    
    [ExportSubgroup("Abilities")]
    [Export] public bool AllowJump = true;
    [Export] public bool AllowSlide = true;
    [Export] public bool AllowDash = true;

    [ExportGroup("Sounds")]
    [Export] public bool OverrideFootstep = false;
    // TODO(calco): Add audio.
}