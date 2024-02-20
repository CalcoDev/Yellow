using Godot;

namespace Yellow.Resources;

[GlobalClass]
public partial class PlayerInput : Resource
{
    public enum KeyState
    {
        Pressed,
        Down,
        Released,
        Up
    }

    [Export] public Vector2 Movement;
    [Export] public KeyState Jump;
    [Export] public KeyState Slide;
    [Export] public KeyState Dash;

    [Export] public KeyState LMB;
    [Export] public KeyState RMB;
}