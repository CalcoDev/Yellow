using Godot;
using Yellow.Misc;
using Yellow.Resources;

namespace Yellow.Managers;

using static PlayerInput;

public partial class InputManager : Node
{
	[ExportGroup("Refernces")]
	[Export] private PlayerInput _input;

	[ExportGroup("Settings")]
	[Export] private bool _update;

	public override void _Ready()
	{
		ProcessPriority = (int)NodeProcessOrder.PlayerInput;
	}

	public override void _Process(double delta)
	{
		if (!_update) {
			return;
		}

		float mx = Input.GetAxis("mx_n", "mx_p");
		float mz = Input.GetAxis("mz_n", "mz_p");
		_input.Movement = new(mx, mz);

		_input.Jump = UpdateKeyState(_input.Jump, Input.IsActionPressed("jump"));
		_input.Dash = UpdateKeyState(_input.Dash, Input.IsActionPressed("dash"));
		_input.Slide = UpdateKeyState(_input.Slide, Input.IsActionPressed("slide"));
	}

	private static KeyState UpdateKeyState(KeyState prev, bool down)
	{
		return prev switch {
			KeyState.Pressed or KeyState.Down => down ? KeyState.Down : KeyState.Released,
			KeyState.Released or KeyState.Up => down ? KeyState.Pressed : KeyState.Up,
			_ => KeyState.Down // unreachable
		};
	}
}
