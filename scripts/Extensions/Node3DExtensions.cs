using Godot;

namespace Yellow.Extensions;

public static class Node3DExtensions
{
    public static Vector3 Forward(this Node3D n) => n.GlobalTransform.Basis.Z;
    public static Vector3 Back(this Node3D n) => -n.GlobalTransform.Basis.Z;
    public static Vector3 Right(this Node3D n) => n.GlobalTransform.Basis.X;
    public static Vector3 Left(this Node3D n) => -n.GlobalTransform.Basis.X;
    public static Vector3 Up(this Node3D n) => n.GlobalTransform.Basis.Y;
    public static Vector3 Down(this Node3D n) => -n.GlobalTransform.Basis.Y;
}