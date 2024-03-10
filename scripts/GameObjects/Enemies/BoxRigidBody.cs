using Godot;
using System;
using Yellow.Components;
using Yellow.Misc;

namespace Yellow.GameObjects.Enemies;

public partial class BoxRigidBody : RigidBody3D	
{
	private Label3D _label;
	private KnockBackComponent _knockBackComponent;
	
	public override void _Ready()
	{
		_label = GetNode("Label3D") as Label3D;
		_knockBackComponent = GetNode("KnockBackComponent") as KnockBackComponent;
	}
	
	public override void _Process(double delta)
	{
		_label.Text = "KR: " + (int)(_knockBackComponent.KnockBackResistance * 100) / 100.0f;
	}
}
