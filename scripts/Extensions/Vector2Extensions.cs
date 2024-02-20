using Godot;

namespace Yellow.Extensions;

public static class Vector2Extensions
{
    public static Vector3 ToVec3(this Vector2 v, float y = 0) => new(v.X, y, v.Y);
}