using Godot;
using System;

namespace Yellow.GameObjects.Enemies;

[GlobalClass]
public partial class Box : Node3D
{
	public bool Killed = false;
	public override void _Ready()
	{
		
	}
	
	public override void _Process(double delta)
	{
		
	}

	public void OnKilled()
	{
		Killed = true;
		GD.Print("Killed box");
		QueueFree();
	}
}
