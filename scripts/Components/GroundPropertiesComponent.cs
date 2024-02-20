using Godot;
using Yellow.Resources;

namespace Yellow.Components;

public partial class GroundPropertiesComponent : Node
{
    [Export] public GroundProperties Properties { get; private set; }
}