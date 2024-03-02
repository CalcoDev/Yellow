using Godot;

namespace Yellow.Nodes;

[GlobalClass]
public partial class DistanceParticles : GpuParticles3D
{
    [ExportGroup("Settings")]
    [Export] public float MinDistance;
    [Export] public int CustomAmount;
    [Export] public new bool Emitting
    {
        get => _isEmittingCustom;
        set {
            _isEmittingCustom = value;
            if (_isEmittingCustom) {
                _distAccum = MinDistance;
                DoParticles();
            }
        }
    }
    private bool _isEmittingCustom = true;

    private Vector3 _prevPos;
    private float _distAccum;

    public override void _EnterTree()
    {
        Emitting = false;
    }

    public override void _Process(double delta)
    {
        if (!Emitting) {
            return;
        }

        _distAccum += _prevPos.DistanceTo(GlobalPosition);
        DoParticles();
        _prevPos = GlobalPosition;
    }

    private void DoParticles()
    {
        if (_distAccum >= MinDistance) {
            var shotCount = Mathf.CeilToInt(_distAccum / MinDistance);
            _distAccum = 0f;
            for (int i = 0; i < shotCount; ++i) {
                var pos = _prevPos.Lerp(GlobalPosition, (float)i / shotCount);
                var cnt = (int) (CustomAmount * Explosiveness);
                for (int j = 0; j < cnt; ++j) {
                    EmitParticle(new Transform3D(Basis, pos), Vector3.Zero, Colors.White, Colors.White, 0);
                }
            }
        }
    }
}