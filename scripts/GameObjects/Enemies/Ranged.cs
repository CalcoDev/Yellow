using Godot;
using Yellow.Managers;
using Yellow.Extensions;
using Yellow.GameObjects.Projectiles;

namespace Yellow.GameObjects.Enemies;

public partial class Ranged : Enemy
{
    [ExportGroup("Ranged Settings")]
    [Export] private float MoveSpeed = 12f;

    [Export] private float ShotRange = 50f;

    [Export] private PackedScene _projectile;
    [Export] private float ShotTime = 0.5f;
    private float _shotTimer;

    public override void _Ready()
    {
        _shotTimer += (float)GD.RandRange(0.0, 0.5);
    }

    public override void _Process(double delta)
    {
        var d = (Player.Instance.GlobalPosition - GlobalPosition).Normalized();
        var dist = (Player.Instance.GlobalPosition - GlobalPosition).Length();

        if (dist > ShotRange) {
            Pathfinding.TargetPosition = Player.Instance.GlobalPosition;
        } else if (dist < 10f) {
            Pathfinding.TargetPosition = GlobalPosition - d * 2f;
        } else {
            Pathfinding.TargetPosition = GlobalPosition;
        }

        Pathfinding.FollowPath();

        _shotTimer -= Game.DeltaTime;
        // if (dist < ShotRange && _shotTimer < 0f) {
        //     Shoot();
        // }
    }

    public override void _PhysicsProcess(double d)
    {
        // var y = LinearVelocity.Y;
        // LinearVelocity = Pathfinding.CachedDir * MoveSpeed;
        if (LinearVelocity.WithY(0).Length() <= MoveSpeed) {
            ApplyForce(Pathfinding.CachedDir * MoveSpeed);
        } else {
            ApplyForce(Pathfinding.CachedDir * -MoveSpeed);
        }
        // LinearVelocity = LinearVelocity.WithY(y);
    }

    private void Shoot()
    {
        _shotTimer = ShotTime + (float)GD.RandRange(0.0f, 0.15f);
        // GD.Print("shot");
        var p = _projectile.Instantiate<FirableProjectile>();
        
        var parent = (Node)this;
        while (true) {
            var p2 = parent.GetParent();
            if (p2 == null) {
                break;
            }
            parent = p2;
        }
        parent.AddChild(p);
        CallDeferred(MethodName.HandleStuff, p);
    }

    private void HandleStuff(FirableProjectile proj)
    {
        proj.GlobalPosition = GlobalPosition + Vector3.Up * 2f;
        proj.Shoot(Player.Instance.GlobalPosition, proj.GlobalPosition, 10f);
    }
}