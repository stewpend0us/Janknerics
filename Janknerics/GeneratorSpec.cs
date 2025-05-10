using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Janknerics;

/// <summary>
/// 
/// </summary>
internal class GeneratorMember(MemberDeclarationSyntax template, TypeSyntax? newType)
{
    public MemberDeclarationSyntax Template = template;
    public TypeSyntax? NewType = newType;
}

/// <summary>
/// everything we need to generate a single output class
/// </summary>
internal class GeneratorSpec
{
    public TypeSyntax? Constructor;
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