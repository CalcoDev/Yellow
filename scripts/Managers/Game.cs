using System;
using Godot;
using Yellow.Components;
using Yellow.Extensions;
using Yellow.Misc;
using Yellow.UI;

namespace Yellow.Managers;

[GlobalClass]
public partial class Game : Node
{
	public static Game Instance { get; private set; }

	[Export] private GeneralMenu _pauseMenu;
	
	// TIME
	public static float DeltaTime { get; private set; }
	public static float FixedDeltaTime { get; private set; }
	public static float Time { get; private set; }
	public static float FixedTime { get; private set; }

	// GAME
	public static CameraComponent PlayerCamera { get; private set; }
	public static CameraComponent ActiveCamera { get; private set; }

	// FULLSCREEN
	public static bool Fullscreen {
		get => _fullscreen;
		set {
			_fullscreen = value;
			var val = _fullscreen ? DisplayServer.WindowMode.Fullscreen : _prevWinMode;
			if (_fullscreen) {
				_prevWinMode = DisplayServer.WindowGetMode();
			}
			DisplayServer.WindowSetMode(val);
		}
	}
	private static bool _fullscreen;
	private static DisplayServer.WindowMode _prevWinMode;

	// MOUSE
	public static bool MouseLocked
	{
		get => _mouseLocked;
		set {
			_mouseLocked = value;
			Input.MouseMode = value ? 
				Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
		}
	}
	private static bool _mouseLocked;

	public override void _EnterTree()
	{
		if (Instance != null)
		{
			GD.PushWarning("WARN: Game Manager already exists!");
			QueueFree();
			return;
		}

		Instance = this;

		// Make this node execute first, always.
		ProcessPriority = (int)NodeProcessOrder.Game;
		ProcessMode = ProcessModeEnum.Always;
	}

	public static bool IsUIScene { get; set; } = false;

	public static bool Paused
	{
		get => _paused;
		set {
			_paused = value;
			if (_paused) {
				GD.Print("GAME IS PUASED");
				Instance.GetTree().Paused = true;
				MouseLocked = false;
				Instance._pauseMenu.SetActive(true);
				Instance._pauseMenu.Visible = true;
			} else {
				Instance.GetTree().Paused = false;
				MouseLocked = true;
				Instance._pauseMenu.SetActive(false);
				Instance._pauseMenu.Visible = false;
			}
		}
	}
	private static bool _paused;

	public override void _Ready()
	{
		if (IsUIScene == false) {
			Paused = false;
		}
	}

	public override void _Process(double delta)
	{
		DeltaTime = (float) delta;
		Time += DeltaTime;

		if (Input.IsActionJustPressed("quit")) {
			GetTree().Quit();
		}

		if (Input.IsActionJustPressed("pause")) {
			if (IsUIScene) {
				return;
			}

			_pauseMenu.ToggleViz();
			Paused = !Paused;
		}
		
		if (Input.IsActionJustPressed("fullscreen")) {
			Fullscreen = !Fullscreen;
		}

		if (Input.IsActionJustPressed("unfocus")) {
			MouseLocked = !MouseLocked;
		}

		if (Input.IsActionJustPressed("screenshot")) {
			Screenshot(GetViewport());
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		FixedDeltaTime = (float) delta;
		FixedTime += FixedDeltaTime;
	}

	// TODO(calco): Sth more
	public static void SetPlayerCamera(CameraComponent cam)
	{
		PlayerCamera = cam;
	}
	
	public static void SetActiveCamera(CameraComponent cam)
	{
		ActiveCamera = cam;
	}

	private static void Screenshot(Viewport viewport)
	{
		DateTime now = DateTime.Now;
		string name = $"Screenshot_{now:yyyy_MM_dd_HH_mm_ss_ffffff}.png";
		
		viewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.Nearest;
		Error err = viewport.GetTexture().GetImage().SavePng($"res://screenshots/{name}");

		if (err != Error.Ok) {
			GD.PushError($"ERROR: Could not save {name}! ({err})");
		}
	}
}
