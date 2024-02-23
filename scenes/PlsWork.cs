using Godot;
using System;
using Yellow.Qodot;

public partial class PlsWork : Node3D
{
	[Export] private QodotCustomTrigger _trigger;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_trigger.OnTriggerEnter += (body) => GD.Print("ENTERED PLS");
		_trigger.OnTriggerExit += (body) => GD.Print("exit PLS");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
