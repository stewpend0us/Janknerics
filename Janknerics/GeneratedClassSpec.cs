using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Janknerics;

public class GeneratedClassSpec
{
    public List<TypeSyntax> Constructor = [];
    public List<TypeSyntax?> Member = [];
}


/*
 * INPUT
 * x
 * a
 * a
 * b
 * Class
 *   a
 *   b
 *   Prop1
 *   b
 *   d
 *   Prop2
 *
 * OUTPUT
 * x (ctor),       ()
 * a (ctor, ctor), (P1)
 * b (ctor),       (P1, P2)
 * d (),           (P2)
 */