using Godot;

namespace Yellow;

// !!! NOTE(calco): TEMPORARY
public partial class PlayerUIManager : Node
{
    [ExportGroup("References")]
    [Export] private ProgressBar[] _staminaBars;

    [Export] private ColorRect _w;
    [Export] private ColorRect _a;
    [Export] private ColorRect _s;
    [Export] private ColorRect _d;
    [Export] private ColorRect _shift;
    [Export] private ColorRect _lctrl;
    [Export] private ColorRect _space;
    
    public void DisplayStamina(float stamina)
    {
        for (int i = 0; i < 3; ++i) {
            float s = Mathf.Clamp(stamina - i, 0, 1f);
            _staminaBars[i].Value = s;
        }
    }

    public override void _EnterTree()
    {
        foreach (var bar in _staminaBars) {
            bar.MinValue = 0f;
            bar.MaxValue = 1f;
            bar.Value = 0;
        }
    }

    public override void _Process(double delta)
    {
        _w.Color = Input.IsActionPressed("mx_p") ? Colors.Red : Colors.White;
        _a.Color = Input.IsActionPressed("mz_p") ? Colors.Red : Colors.White;
        _s.Color = Input.IsActionPressed("mx_n") ? Colors.Red : Colors.White;
        _d.Color = Input.IsActionPressed("mz_n") ? Colors.Red : Colors.White;
        _shift.Color = Input.IsActionPressed("dash") ? Colors.Red : Colors.White;
        _lctrl.Color = Input.IsActionPressed("slide") ? Colors.Red : Colors.White;
        _space.Color = Input.IsActionPressed("jump") ? Colors.Red : Colors.White;
    }
}