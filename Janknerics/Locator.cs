using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Janknerics;

/// <summary>
/// given a namespace:
/// 1) locate any types that contain Jankneric attributes
/// 2) break the types up as needed until there is 1 TypdDeclarationSyntax per generated type
/// </summary>
public class Locator : CSharpSyntaxVisitor<IEnumerable<TypeDeclarationSyntax>>
{
    /// <summary>
    /// visit all members of any BaseNamespaceDeclarationSyntax we encounter
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public override IEnumerable<TypeDeclarationSyntax>? DefaultVisit(SyntaxNode node)
    {
        if (node is BaseNamespaceDeclarationSyntax ns)
            foreach (var m in ns.Members)
                if (base.Visit(m) is { } visit)
                    foreach (var v in visit)
                        yield return v;

        if (base.DefaultVisit(node) is { } result)
            foreach (var r in result)
                yield return r;
    }

    //private static readonly Rewriter Rewriter = new();
    
    /// <summary>
    /// visit any TypeDeclarationSyntax we encounter
    /// if it contains any Jankneric attributes then split it into the corresponding number of output types
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private static IEnumerable<TypeDeclarationSyntax> VisitTypeDeclaration(TypeDeclarationSyntax node)
    {
        // figure out if it's marked with any Jankneric attributes
        var containsJankneric = node.AttributeLists
            .Where(list => list.Attributes
                .Any(attribute => attribute.Name.ToString().Equals("Jankneric")))
            .ToArray();

        if (containsJankneric.Length == 0)
            yield break; // none. we're done.
        
        // break the input definition up into 1 spec per output
        node = node.RemoveNodes(containsJankneric, SyntaxRemoveOptions.KeepNoTrivia)!;
        var result = Split(containsJankneric, node);
        foreach (var r in result)
            if (r is not null)
                yield return r;
    }

    private static SyntaxToken CheckArgument(AttributeArgumentSyntax argument)
    {
        if (argument.Expression is not TypeOfExpressionSyntax typeOfExpression)
            throw new CustomAttributeFormatException("All arguments of Jankneric attribute must be 'typeof' expression");
        return SyntaxFactory.Identifier(typeOfExpression.Type.ToString());
    }

    private static IEnumerable<TypeDeclarationSyntax> Split(IEnumerable<AttributeListSyntax> containsJankneric, TypeDeclarationSyntax node)
    {
        var attributeLists= containsJankneric.ToList();
        foreach (var list in attributeLists)
        {
            if (list is null)
                continue;
            
            foreach (var attributeGroup in list.Attributes
                         .Where(attr => attr.ArgumentList is not null)
                         .GroupBy(attr => attr.ArgumentList!.Arguments.Count))
            {
                switch (attributeGroup.Key) // Count
                {
                    case 0:
                        throw new CustomAttributeFormatException("Jankneric attribute must have at least one argument");
                    default:
                        throw new CustomAttributeFormatException("Jankneric attribute must have at most two arguments");
                    case 1 or 2:
                        foreach (var attribute in attributeGroup)
                        {
                            var destinationType = CheckArgument(attribute.ArgumentList!.Arguments[0]);
                            SyntaxTokenList args = new ();
                            if (attributeGroup.Key > 1)
                            {
                                
                                var arg = CheckArgument(attribute.ArgumentList!.Arguments[1]);
                                args = args.Add(arg);
                            }

                            yield return Rewrite(node, destinationType, args)
                                .NormalizeWhitespace();
                        }

                        break;
                }
            }
        }
    }

    private static TypeDeclarationSyntax Rewrite(TypeDeclarationSyntax node, SyntaxToken destinationType, SyntaxTokenList args)
    {
        // TODO remove/modify members with Jankneric Attributes
        foreach (var member in node.Members)
        {
            foreach (var list in member.AttributeLists)
            {
                
            }
        }
        //foreach (var member in node.AttributeLists)
        return node.WithIdentifier(destinationType)
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword));
    }

    public override IEnumerable<TypeDeclarationSyntax>? VisitClassDeclaration(ClassDeclarationSyntax node) =>
        VisitTypeDeclaration(node);   

    public override IEnumerable<TypeDeclarationSyntax>? VisitStructDeclaration(StructDeclarationSyntax node) =>
        VisitTypeDeclaration(node);

    public override IEnumerable<TypeDeclarationSyntax>? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) =>
        VisitTypeDeclaration(node);

    public override IEnumerable<TypeDeclarationSyntax>? VisitRecordDeclaration(RecordDeclarationSyntax node) =>
        VisitTypeDeclaration(node);

//    public override void VisitAttribute(AttributeSyntax node)
//    {
//        if (node.Name.ToString() != "Jankneric")
//            return base.VisitAttribute(node);
//        if (node.Parent is not AttributeListSyntax list)
//            throw new NotImplementedException();
//        var updatedList = list.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia)!;
//        if (updatedList.Attributes.Count > 0)
//            return VisitAttributeList(updatedList);
//        if (list.Parent is FieldDeclarationSyntax member)
//            return MyFieldDeclaration(member);
//        return null;
//    }

   // private SyntaxNode MyFieldDeclaration(FieldDeclarationSyntax node)
   // {
   //     return Visit(node.WithDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("string"),node.Declaration.Variables)));
   // }

    //public override SyntaxNode? VisitAttributeList(AttributeListSyntax node)
    //{
    //    return base.VisitAttributeList(node);
    //}
    //    // if they're all Jankneric Attributes then we remove the whole list
    //    if (node.Attributes.All(attr => attr.Name.ToString() == "Jankneric"))
    //        return null;
    //    
    //    // otherwise we remove only the Jankneric attributes from the list
    //    foreach (var attribute in node.Attributes.Where(attr => attr.Name.ToString() == "Jankneric"))
    //    {
    //        var result = node.RemoveNode(attribute, SyntaxRemoveOptions.KeepNoTrivia);
    //        if (result is null)
    //            return null; //TODO error??
    //        node = result;
    //    }

    //    // continue the recursion
    //    return base.VisitAttributeList(node);
    //}
}