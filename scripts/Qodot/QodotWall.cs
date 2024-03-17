using Godot;
using Godot.Collections;

namespace Yellow.Qodot;

[Tool]
public partial class QodotWall : Node3D
{
    [ExportGroup("QODOT!!! DO NOT TOUCH")]
	[Export] public Dictionary properties;

    private string _leProp;

    public override void _Ready()
    {
        _leProp = null;
        if (properties.TryGetValue("transparent", out var variant)) {
            _leProp = variant.As<string>();
        }
        
        Visible = _leProp == "true";
    }
}