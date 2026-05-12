using System;

namespace TLD.TinyJSON;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class Include : Attribute
{
}
