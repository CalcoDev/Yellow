
using Godot;

namespace Yellow.Extensions;

public static class Vector3Extensions
{
    public static Vector3 WithX(this Vector3 v, float x) => new(x, v.Y, v.Z);
    public static Vector3 WithY(this Vector3 v, float y) => new(v.X, y, v.Z);
    public static Vector3 WithZ(this Vector3 v, float z) => new(v.X, v.Y, z);

    public static bool DotLess(this Vector3 a, Vector3 b, float epsilon, bool abs = true) {
        var dot = a.Dot(b);
        return a.LengthSquared() > 0.01 && b.LengthSquared() > 0.01 && (abs ? Mathf.Abs(dot) : dot) < epsilon;
    }
    
    public static bool DotGreater(this Vector3 a, Vector3 b, float epsilon, bool abs = true) {
        var dot = a.Dot(b);
        return a.LengthSquared() > 0.01 && b.LengthSquared() > 0.01 && (abs ? Mathf.Abs(dot) : dot) > epsilon;
    }
}