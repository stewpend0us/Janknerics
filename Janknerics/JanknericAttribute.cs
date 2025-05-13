using System.Diagnostics.CodeAnalysis;

namespace Janknerics;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public sealed class JanknericAttribute(Type targetType) : Attribute
{
    internal const string Name = "Jankneric";
    public Type TargetType => targetType;
    public Type? NewType { get; set; }
    public ConversionMethod ConversionMethod { get; set; } = ConversionMethod.Automatic;
    public string? ConversionFunctionName { get; set; }
}

public enum ConversionMethod
{
    Automatic,
    Assign,
    Cast,
    Construct,
    Specified
}

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class JanknericConstructorAttribute(Type targetType) : Attribute
{
    internal const string Name = "JanknericConstructor";
    
    public Type TargetType => targetType;
}
