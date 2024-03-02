using Godot;

namespace Yellow.Nodes;

[GlobalClass]
[Tool]
public partial class DistanceParticles : GpuParticles3D
{
    [ExportGroup("Settings")]
    [Export] public float MinDistance;

    private Vector3 _prevPos;
    private float _distAccum;

    public override void _Process(double delta)
    {
        var refPos = GlobalPosition;
        _distAccum += _prevPos.DistanceTo(refPos);
        if (_distAccum >= MinDistance) {
            var particleCount = Mathf.CeilToInt(_distAccum / MinDistance);
            _distAccum = 0f;
            for (int i = 0; i < particleCount; ++i) {
                var pos = _prevPos.Lerp(refPos, (float)i / particleCount);
                EmitParticle(new Transform3D(Basis, pos), Vector3.Zero, Colors.White, Colors.White, 0);
            }
        }
        _prevPos = refPos;
    }
}