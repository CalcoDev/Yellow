using Godot;
using System.Collections.Generic;
using System.Linq;
using Yellow.GameObjects.Enemies;

namespace Yellow.Misc;

[GlobalClass]
public partial class WeaponTestWall : Node3D
{
	[Export] public int MaximumBoxCount = 5;

	private List<Box> _existingBoxes;
	private PackedScene _boxScene;
	
	public override void _Ready()
	{
		_existingBoxes = new List<Box>();
		_boxScene = ResourceLoader.Load<PackedScene>("res://scenes/game_objects/entities/box.tscn");
	}

	public override void _Process(double delta)
	{
		CheckForKilled();
		if(_existingBoxes.Count < MaximumBoxCount) SpawnNewBox();
	}

	private void SpawnNewBox()
	{
		var boxLocation = new Vector3(0, 0, 0.5f);
		boxLocation.X += (float)GD.RandRange(-2.0f, 2.0f);
		boxLocation.Y += (float)GD.RandRange(-2.0f, 2.0f);
		
		_existingBoxes.Add(_boxScene.Instantiate() as Box);
		
		_existingBoxes.Last().Position = boxLocation;
		AddChild(_existingBoxes.Last());
	}

	private void CheckForKilled()
	{
		for (var i = _existingBoxes.Count - 1; i >= 0; i--)
		{
			if (_existingBoxes[i].Killed == true) 
				_existingBoxes.RemoveAt(i);
		}
	}
}
