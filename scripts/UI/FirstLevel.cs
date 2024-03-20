using Godot;
using System;
using Yellow.Managers;

public partial class FirstLevel : Node
{
    public static SoundManager.SoundIdentifier MAIN_THEME;

    [Export] private SpotLight3D _f1;
    [Export] private SpotLight3D _f2;
    [Export] private SpotLight3D _f3;

    public void _on_entity_10_mover_on_start_move(Variant f)
    {
        _f2.Visible = true;
    }

    public void _on_entity_14_mover_on_start_move(Variant f)
    {
        _f1.Visible = true;
    }

    public void _on_entity_20_mover_on_start_move(Variant f)
    {
        _f3.Visible = true;
    }

    public override void _Ready()
    {
        _f1.Visible = false;
        _f2.Visible = false;
        _f3.Visible = false;

        SoundManager.Instance.Stop(MainMenu.SOUND_IDENT);
        MAIN_THEME = SoundManager.Instance.Play("main_theme");
        var c = Game.Instance.GetNode<ColorRect>("%color").Color;
		c.A = 127;
		Game.Instance.GetNode<ColorRect>("%color").Color = c;
		Game.Instance._pauseMenu._isMain = false;
		Game.Instance._pauseMenu._gameplayScene = null;
    }
}
