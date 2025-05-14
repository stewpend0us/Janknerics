using Janknerics.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Janknerics;

/// <summary>
/// 
/// </summary>
internal class GeneratorMember(MemberDeclarationSyntax template, TypeSyntax? newType, ConversionMethod method, string? customMethodName)
{
    public readonly MemberDeclarationSyntax Template = template;
    public readonly TypeSyntax? NewType = newType;
    public ConversionMethod Method = method;
    public string? CustomMethodName = customMethodName;
}

internal class GeneratorConstructor(TypeSyntax? type)
{
    public TypeSyntax? Type = type;
}

/// <summary>
/// everything we need to generate a single output class
/// </summary>
internal class GeneratorSpec
{
    public GeneratorConstructor? Constructor;
    public readonly List<GeneratorMember> Members = [];
}


/*
 * INPUT
 * x
 * a
 * b
 * Class
 *   a
 *   b
 *   P1
 *   b
 *   d
 *   P2
 *
 * OUTPUT
 * x (ctor), ()
 * a (ctor), (P1)
 * b (ctor), (P1, P2)
 * d (),     (P2)
 */