using Godot;
using Yellow.Managers;
using Yellow.Extensions;

namespace Yellow.GameObjects.Enemies;

public partial class Accensus : Enemy
{
    [ExportGroup("Accensus Settings")]
    [Export] private float MoveSpeed = 12f;
    
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
        _anim.Play("idle");
        // _anim.AnimationFinished += (StringName name) => {
        //     GD.Print("FNIIHSIEDL ", name);
        // };
    }

    public override void _Process(double delta)
    {
        if (_isLungeAnim) {
            return;
        }

        if (!ShouldDoStuff) {
            _anim.Play("idle");
            return;
        }

        var d = (Player.Instance.GlobalPosition - GlobalPosition).Normalized();

        _anim.Play("chase");
        _nav.TargetPosition = Player.Instance.GlobalPosition - d * (LungeRange - 1f);
        _nav.FollowPath();
        
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
            LinearVelocity = _nav.CachedDir * MoveSpeed;
        } else {
            LinearVelocity = (_lungeDir * LungeDistance) / _anim.GetAnimation("lunge").Length;
        }
        LinearVelocity = LinearVelocity.WithY(y);
    }

    private Transform3D _frickThis;

    private async void StartLunge()
    {
        _stimer = -0.2f;
        GD.Print("START LUNGE CHARGE");
        LinearVelocity = Vector3.Zero;
        _isLungeAnim = true;

        _lungeDir = (Player.Instance.GlobalPosition - GlobalPosition).WithY(0).Normalized();

        _anim.Stop();
        _anim.Play("lunge_charge");
        await ToSignal(_anim, AnimationPlayer.SignalName.AnimationFinished);
        GD.Print("END LUNGE CHARGE");

        GD.Print("START LUNGE");

        IsLunging = true;
        _anim.Stop();
        _anim.Play("lunge");

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
        // }), 0.0f, 2.0f, _anim.GetAnimation("lunge").Length);
        // t.Play();
        await ToSignal(_anim, AnimationPlayer.SignalName.AnimationFinished);
        // t.Kill();
        
        // _doFrickThis = true;
        // _frickThis = GetNode<Node3D>("_localPos").GlobalTransform;

        GD.Print("END LUNGE");
        IsLunging = false;

        _isLungeAnim = false;
        
        _lungeTimer = LungeCooldown;
    }
}