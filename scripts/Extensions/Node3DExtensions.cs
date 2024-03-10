using Godot;

namespace Yellow.Extensions;

public static class Node3DExtensions
{
    public static Vector3 Forward(this Node3D n) => n.GlobalTransform.Basis.Z.Normalized();
    public static Vector3 Back(this Node3D n) => -n.GlobalTransform.Basis.Z.Normalized();
    public static Vector3 Right(this Node3D n) => n.GlobalTransform.Basis.X.Normalized();
    public static Vector3 Left(this Node3D n) => -n.GlobalTransform.Basis.X.Normalized();
    public static Vector3 Up(this Node3D n) => n.GlobalTransform.Basis.Y.Normalized();
    public static Vector3 Down(this Node3D n) => -n.GlobalTransform.Basis.Y.Normalized();

    public static Vector3 ForwardXZ(this Node3D n) => n.GlobalTransform.Basis.Z.WithY(0).Normalized();
    public static Vector3 RightXZ(this Node3D n) => n.GlobalTransform.Basis.X.WithY(0).Normalized();
    public static Vector3 UpXZ(this Node3D n) => n.GlobalTransform.Basis.Y.WithY(0).Normalized();
}