// Polyfill for NullableAttribute / NullableContextAttribute when Il2Cppmscorlib
// shadows the BCL System.Runtime.CompilerServices namespace.
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum |
                    AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property |
                    AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface |
                    AttributeTargets.Parameter | AttributeTargets.GenericParameter | AttributeTargets.Delegate,
                    AllowMultiple = false, Inherited = false)]
    internal sealed class NullableAttribute : Attribute
    {
        public readonly byte[] NullableFlags;
        public NullableAttribute(byte b) { NullableFlags = new[] { b }; }
        public NullableAttribute(byte[] b) { NullableFlags = b; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface |
                    AttributeTargets.Delegate | AttributeTargets.Method | AttributeTargets.Constructor |
                    AttributeTargets.Module | AttributeTargets.Event | AttributeTargets.Property,
                    AllowMultiple = false, Inherited = false)]
    internal sealed class NullableContextAttribute : Attribute
    {
        public readonly byte Flag;
        public NullableContextAttribute(byte b) { Flag = b; }
    }
}
