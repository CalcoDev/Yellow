using Godot;
using Godot.Collections;

namespace Yellow.Qodot;

public partial class QodotSpawn : Node3D
{
    [ExportGroup("QODOT!!! DO NOT TOUCH")]
	[Export] public Dictionary properties;

    private string _leProp;

    public override void _Ready()
    {
        _leProp = null;
        if (properties.TryGetValue("scene", out var variant)) {
            _leProp = variant.As<string>();
        }
        
        if (_leProp != null && _leProp != "") {
            var scene = GD.Load<PackedScene>($"res://scenes/{_leProp}");
            var node = scene.Instantiate();
            if (node is Node3D n3d) {
                n3d.GlobalTransform = GlobalTransform;
            }
        }
    }
}