using Godot;

namespace Yellow;

public partial class PlayerUIManager : Node
{
    [ExportGroup("References")]
    [Export] private ProgressBar[] _staminaBars;
    
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
}