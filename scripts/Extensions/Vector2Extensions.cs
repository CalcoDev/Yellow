using Godot;

namespace Yellow.Extensions;

public static class Vector2Extensions
{
    public static Vector3 ToVec3(this Vector2 v, float y = 0) => new(v.X, y, v.Y);
    
    public static Vector3 SetX(this Vector3 v, float x = 0) => new(x, v.Y, v.Z);
    public static Vector3 SetY(this Vector3 v, float y = 0) => new(v.X, y, v.Z);
    public static Vector3 SetZ(this Vector3 v, float z = 0) => new(v.X, v.Y, z);
}