using Godot;
using System;

public partial class Arrow : RigidBody3D
{
	public Vector3 Trajectory;
	
	public override void _Ready()
	{
		
	}
	
	public override void _Process(double delta)
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{
		ApplyForce(Trajectory);
		Trajectory *= 0.8f;
	}
}
