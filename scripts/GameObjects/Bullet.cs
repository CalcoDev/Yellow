using Godot;
using System;
using Yellow.Resources.Items;

public partial class Bullet : Node
{
	[Export] public MeshInstance3D Model;
	[Export] public CollisionShape3D Collider;

	private Vector3 _velocity;
	private float _speed;
	
	public override void _Ready()
	{
		
	}
	
	public override void _Process(double delta)
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{
		
	}
}
