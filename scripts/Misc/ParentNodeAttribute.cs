// !!! DISCLAIMER !!!
// The contents of this file were taken from Firebelley's GodotUtilities project, which can be found HERE:
//
// Firebelley: https://github.com/firebelley
// [GodotUtilities](https://github.com/firebelley/GodotUtilities)


using System;

namespace Yellow.Misc;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class ParentAttribute : Attribute
{
    public ParentAttribute() { }
}