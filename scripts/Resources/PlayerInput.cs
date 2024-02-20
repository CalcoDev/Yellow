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

    [Export] public Vector2 Movement = Vector2.Zero;
    [Export] public KeyState Jump = KeyState.Up;
    [Export] public KeyState Slide = KeyState.Up;
    [Export] public KeyState Dash = KeyState.Up;

    [Export] public KeyState LMB = KeyState.Up;
    [Export] public KeyState RMB = KeyState.Up;
}