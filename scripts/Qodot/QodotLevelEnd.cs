using Godot;
using Godot.Collections;
using System;
using Yellow.GameObjects;

namespace Yellow.Qodot;

[GlobalClass]
public partial class QodotLevelEnd : Area3D
{
	[ExportGroup("QODOT!!! DO NOT TOUCH")]
	[Export] public Dictionary properties;

    private string _leProp;

    public override void _Ready()
    {
        CollisionMask = 1024;
        BodyEntered += OnBodyEntered;

        _leProp = null;
        if (properties.TryGetValue("scene", out var variant)) {
            _leProp = variant.As<string>();
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is not Player) {
            return;
        }

        if (_leProp != null && _leProp != "") {
            GetTree().ChangeSceneToFile($"res://scenes/{_leProp}");
        }
    }
}