using Godot;
using Yellow.Extensions;
using Yellow.Managers;

namespace Yellow.UI;

[GlobalClass]
public partial class GeneralMenu : Control
{
    [Export] public bool _isMain;
    [Export] public PackedScene _gameplayScene;

    [Export] private Button _resume;
    [Export] private Button _options;
    [Export] private Button _quit;

    [Export] private Slider _musicSlider;
    [Export] private Slider _sfxSlider;
    [Export] private Button _optionsBack;

    [Export] private Control _main;
    [Export] private Control _opt;

    public void ToggleViz() {
		Visible = !Visible;
    }

    public override void _EnterTree()
    {
        if (_isMain) {
            Game.IsUIScene = true;
            Game.Paused = true;
        }
    }

    public override void _Ready()
    {
        _musicSlider.ValueChanged += (value) => {
            // value -= 0.12f;
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(SoundManager.MusicAudioBus), Mathf.LinearToDb((float)value));
        };
        
        _sfxSlider.ValueChanged += (value) => {
            // value -= 0.18f;
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(SoundManager.SFXAudioBus), Mathf.LinearToDb((float)value));
        };

        _resume.ButtonDown += () => {
            if (_isMain) {
                GetTree().ChangeSceneToPacked(_gameplayScene);
            } else {
                Visible = false;
            }
            
            Game.IsUIScene = false;
            Game.Paused = false;
        };
        _options.ButtonDown += () => {
            _opt.Visible = true;
            _main.Visible = false;
        };
        _quit.ButtonDown += () => {
            GetTree().Quit();
        };

        _optionsBack.ButtonDown += () => {
            _opt.Visible = false;
            _main.Visible = true;
        };
    }
}