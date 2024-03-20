using Godot;
using Yellow.Managers;
using Yellow.Extensions;
using Yellow.Components;

namespace Yellow.GameObjects.Enemies;

public partial class Accensus : Enemy
{
    [ExportGroup("Accensus Settings")]
    [Export] private float MoveSpeed = 12f;
    [Export] private HitboxComponent _hitbox;
    
    [ExportSubgroup("Lunge")]
    [Export] private float LungeRange = 5f;
    [Export] private float LungeCooldown = 0.1f;
    [Export] private float LungeDistance = 5f;

    private bool _isLungeAnim = false;

    public bool IsLunging { get; private set; } = false;

    private float _lungeTimer = 0f;

    private Vector3 _lungeDir = Vector3.Zero;

    private float _stimer = 0f;

    public override void _Ready()
    {
        Anim.Play("idle");

        _hitbox.CollisionLayer = 0;
        _hitbox.CollisionMask = 0;

        base._Ready();
    }

    public override void _Process(double delta)
    {
        if (_isLungeAnim) {
            return;
        }

        if (!ShouldDoStuff) {
            Anim.Play("idle");
            return;
        }

        var d = (Player.Instance.GlobalPosition - GlobalPosition).Normalized();

        Anim.Play("chase");
        Pathfinding.TargetPosition = Player.Instance.GlobalPosition - d * (LungeRange - 1f);
        Pathfinding.FollowPath();
        
        if (!IsLunging) {
            _lungeTimer -= Game.DeltaTime;
        }

        var dist = Player.Instance.GlobalPosition.DistanceTo(GlobalPosition);
        if (dist < LungeRange && _lungeTimer < 0f && !_isLungeAnim) {
            StartLunge();
        }

        _stimer += Game.DeltaTime;
    }

    private bool _doFrickThis = false;
    private float _aaaa = 0f;

    public override void _PhysicsProcess(double d)
    {
        var y = LinearVelocity.Y;
        if (!_isLungeAnim) {
            LinearVelocity = Pathfinding.CachedDir * MoveSpeed;
        } else {
            LinearVelocity = _lungeDir * LungeDistance / Anim.GetAnimation("lunge").Length;
        }
        LinearVelocity = LinearVelocity.WithY(y);
    }

    private Transform3D _frickThis;

    private async void StartLunge()
    {
        _stimer = -0.2f;
        // GD.Print("START LUNGE CHARGE");
        LinearVelocity = Vector3.Zero;
        _isLungeAnim = true;

        _lungeDir = (Player.Instance.GlobalPosition - GlobalPosition).WithY(0).Normalized();

        _hitbox.CollisionLayer = 1048576;
        _hitbox.CollisionMask = 262144;


        Anim.Stop();
        Anim.Play("lunge_charge");
        await ToSignal(Anim, AnimationPlayer.SignalName.AnimationFinished);
        // GD.Print("END LUNGE CHARGE");

        // GD.Print("START LUNGE");

        IsLunging = true;
        Anim.Stop();
        Anim.Play("lunge");

        // var t = GetTree().CreateTween();
        // var s = GetNode<Node3D>("_localPos").Position;
        // var e = GetNode<Node3D>("_localPos").Position + _lungeDir * LungeDistance;
        // var m = (s + e) / 2f + Vector3.Up;
        
        // t.TweenMethod(Callable.From((float t) => {
        //     var t1 = Mathf.Abs(t - Mathf.Floor(t));
        //     GD.Print($"T: {t} | T1: {t1}");

        //     var aa = s;
        //     var bb = m;
        //     if (t >= 1.0f) {
        //         aa = m;
        //         bb = e;
        //     }
        //     GetNode<Node3D>("_localPos").Position = aa.Lerp(bb, t1);
        // }), 0.0f, 2.0f, Anim.GetAnimation("lunge").Length);
        // t.Play();
        await ToSignal(Anim, AnimationPlayer.SignalName.AnimationFinished);
        // t.Kill();
        
        // _doFrickThis = true;
        // _frickThis = GetNode<Node3D>("_localPos").GlobalTransform;
        
        _hitbox.CollisionLayer = 0;
        _hitbox.CollisionMask = 0;

        GD.Print("END LUNGE");
        IsLunging = false;

        _isLungeAnim = false;
        
        _lungeTimer = LungeCooldown;
    }
}