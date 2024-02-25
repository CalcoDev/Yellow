using Godot;
using Godot.Collections;
using System;

namespace Yellow.Qodot;

[GlobalClass]
public partial class QodotCustomTrigger : Area3D
{
	[ExportGroup("QODOT!!! DO NOT TOUCH")]
	[Export] public Dictionary properties;
	public bool OneShot { get; private set; } = false;
	public bool OneShotOver { get; private set; } = false;

	[Signal]
	public delegate void OnTriggerEnterEventHandler(PhysicsBody3D body);

	[Signal]
	public delegate void OnTriggerExitEventHandler(PhysicsBody3D body);







	// TORFO(calco): WHAT THE FRICK IS THIS
	[Signal]
	public delegate void OnTriggerEnterParamlessEventHandler();

	[Signal]
	public delegate void OnTriggerExitParamlessEventHandler();






	private uint _layerMask;


	public override void _Ready()
	{
		ParseLayers();
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
		
		if (properties.TryGetValue("one_shot", out var oneshot)) {
			OneShot = oneshot.As<string>() == "true";
		}
	}

    private void OnBodyEntered(Node3D body)
    {
		if (body is not PhysicsBody3D pBody || (OneShot && OneShotOver)) {
			return;
		}
		
		OneShotOver = true;
		EmitSignal(SignalName.OnTriggerEnter, pBody);
		EmitSignal(SignalName.OnTriggerEnterParamless);
    }

    private void OnBodyExited(Node3D body)
    {
		if (body is not PhysicsBody3D pBody || (OneShot && OneShotOver)) {
			return;
		}

		OneShotOver = true;
		EmitSignal(SignalName.OnTriggerExit, pBody);
		EmitSignal(SignalName.OnTriggerExitParamless);
    }

	private static readonly System.Collections.Generic.Dictionary<string, int> LayerDict = new();
    private void ParseLayers()
	{
		if (properties == null || !properties.TryGetValue("layer_mask", out Variant stringVariant)) {
			return;
		}

		// Build string dict
		if (LayerDict.Count == 0) {
			for (int i = 1; i < 33; ++i) {
				var name = ProjectSettings.GetSetting($"layer_names/3d_physics/layer_{i}", "NO_NAME").As<String>();
				LayerDict[name.ToLower()] = i - 1;
			}
		}

		_layerMask = 0;
		string str = stringVariant.As<string>();
		foreach (string layer in str.Split(",")) {
			string trimmed = layer.Trim().ToLower();
			if (LayerDict.ContainsKey(trimmed)) {
				_layerMask += ((uint)1) << LayerDict[trimmed];
			}
		}

		CollisionMask = _layerMask;
	}
}
