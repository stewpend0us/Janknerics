using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Janknerics;

/// <summary>
/// 
/// </summary>
public class GeneratedClassMember
{
    public MemberDeclarationSyntax template;
    public TypeSyntax? newType;
}

/// <summary>
/// everything we need to generate a single output class
/// </summary>
public class GeneratedClassSpec
{
    public TypeSyntax? Constructor;
    public List<GeneratedClassMember> Members = [];
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