using Godot;
using Godot.Collections;
using Yellow.Resources;

namespace Yellow.Components;

public partial class BitTagComponent : Node
{
    [Export] private Array<BitTag> _tags;

    public bool HasTag(BitTag bitTag)
    {
        return _tags.Contains(bitTag);
    }
    
    public bool HasTagString(string bitTag)
    {
        foreach (var bitTagAsset in _tags)
        {
            if (bitTagAsset.Tag == bitTag)
                return true;
        }

        return false;
    }
    
    public void AddTag(BitTag bitTag)
    {
        if (!HasTag(bitTag))
            _tags.Add(bitTag);
    }
}