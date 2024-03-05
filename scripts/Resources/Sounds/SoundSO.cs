using Godot;
using Godot.Collections;

namespace Yellow.Resources.Sounds;

[GlobalClass]
public partial class SoundSO : Resource
{
    [Export] public string Name;
    [Export] public SoundLayer Layer = SoundLayer.SFX;
    [Export] public SoundPoolType PoolType = SoundPoolType.Random;
    
    [Export] public bool IsSpatial = true;
    [Export] public bool Looping = false;

    [Export] public float Volume = 1f;
    

    // TODO(calco): Resource should be either AudioStream or another SoundSO
    [Export] public Array<AudioStream> Streams;
}