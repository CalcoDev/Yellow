using Godot;
using System;
using Yellow.GameObjects;

namespace Yellow.Components;

public struct KnockBackData
{
	public Node3D Source;
	public Vector3 KnockBackAxisMultipliers;
	public float KnockBackFactor;
	public bool CheckAirborne;

	public KnockBackData(
		Node3D source, 
		Vector3 knockBackAxisMultipliers, 
		float knockBackFactor,
		bool checkAirborne = false
	)
	{
		Source = source;
		KnockBackAxisMultipliers = knockBackAxisMultipliers;
		KnockBackFactor = knockBackFactor;
		CheckAirborne = checkAirborne;
	}
}

[GlobalClass]
public partial class KnockBackComponent : Node
{
	[ExportGroup("References")]
	[Export] private RigidBody3D _theEntity;
	
	[ExportGroup("KnockBack Modifiers")]
	[Export] private float _kbResFloor = 0.25f;
	[Export] private float _constantMultiplier = 500;

	public float KnockBackResistance { get; private set; }
	private Vector3 _appliedKnockBackForce = Vector3.Zero;
	
	public override void _Ready()
	{
		KnockBackResistance = _kbResFloor;
	}
	
	public override void _Process(double delta)
	{
		KnockBackResistance = Math.Max(_kbResFloor, KnockBackResistance - (float)delta * 0.25f);
	}

	public override void _PhysicsProcess(double delta)
	{
		_theEntity?.ApplyForce(_appliedKnockBackForce);
		_appliedKnockBackForce *= 0.01f;
	}

	public void ApplyKnockBack(KnockBackData data)
	{
		KnockBackResistance = Math.Min(1.0f, KnockBackResistance + 0.1f * data.KnockBackFactor);

		_appliedKnockBackForce =
			(_theEntity.GlobalPosition - data.Source.GlobalPosition).Normalized()
			* data.KnockBackAxisMultipliers
			* _constantMultiplier
			* KnockBackResistance;
	}
}
