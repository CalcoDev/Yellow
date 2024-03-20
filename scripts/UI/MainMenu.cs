using Godot;
using System;
using Yellow.Managers;

public partial class MainMenu : Node
{
	[Export] private PackedScene _scene;
	public static SoundManager.SoundIdentifier SOUND_IDENT;

    public override void _Ready()
    {
        SOUND_IDENT = SoundManager.Instance.Play("main_menu_theme");
		Game.Paused = true;
		Game.IsUIScene = true;
		var c = Game.Instance.GetNode<ColorRect>("%color").Color;
		c.A = 255;
		Game.Instance.GetNode<ColorRect>("%color").Color = c;
		Game.Instance._pauseMenu._isMain = true;
		Game.Instance._pauseMenu._gameplayScene = _scene;
    }
}
