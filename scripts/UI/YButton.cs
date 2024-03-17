using Godot;
using Yellow.Managers;

namespace Yellow.UI;

[GlobalClass]
public partial class YButton : Button
{
    public override void _Ready()
    {
        ButtonDown += () => {
            SoundManager.Instance.Play("button_click");
        };
        MouseEntered += () => {
            SoundManager.Instance.Play("button_hover");
        };
    }
}