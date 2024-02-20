
using Godot;

namespace Yellow.Extensions;

public static class Vector3Extensions
{
    public static Vector3 WithY(this Vector3 v, float y = 0) => new(v.X, y, v.Y);
}