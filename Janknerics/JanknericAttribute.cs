using System.Diagnostics.CodeAnalysis;

namespace Janknerics;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public sealed class JanknericAttribute(Type targetType) : Attribute
{
    internal const string Name = "Jankneric";
    public Type TargetType => targetType;
    public Type? ReplacementType { get; set; }
}

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class JanknericConstructorAttribute(Type targetType) : Attribute
{
    internal const string Name = "JanknericConstructor";
    public Type TargetType => targetType;
}
