using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Janknerics;

public sealed class JanknericsRewriter : CSharpSyntaxRewriter
{

    private static readonly JanknericsRewriter Rewriter = new ();
    public static void Rewrite(BaseNamespaceDeclarationSyntax ns, Action<string, string> writer)
    {
        Rewriter._sourceName = string.Empty;
        
        var result = Rewriter.Visit(ns);

        if (Rewriter._sourceName == string.Empty)
            return;
        
        if (Rewriter._constructorArgType is not null)
            result = ApplyConstructor(result, (SyntaxToken)Rewriter._constructorArgType);

        var source = result.NormalizeWhitespace().ToString();
        writer(Rewriter._sourceName, source);
    }

    private static SyntaxNode ApplyConstructor(SyntaxNode result, SyntaxToken constructorArgType)
    {
        // TODO
        return result;
    }

    private SyntaxToken? _constructorArgType;
    
    private string _sourceName = "";

    private SyntaxToken CheckArgument(AttributeArgumentSyntax argument)
    {
        if (argument.Expression is not TypeOfExpressionSyntax typeOfExpression)
            throw new CustomAttributeFormatException(
                "All arguments of Jankneric attribute must be 'typeof' expression");
        return SyntaxFactory.Identifier(typeOfExpression.Type.ToString());
    }

    private TypeDeclarationSyntax? VisitTypeDeclaration(TypeDeclarationSyntax node)
    {
        TypeDeclarationSyntax? result = null;
        // figure out if it's marked with any Jankneric attributes
        foreach (var attributeList in node.AttributeLists)
        {
            var janknericEnumerable = attributeList.Attributes.Where(attr => attr.Name.ToString() == "Jankneric");
            var janknericAttributes = janknericEnumerable as AttributeSyntax[] ?? janknericEnumerable.ToArray();

            foreach (var attributeGroup in janknericAttributes
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
                    {
                        foreach (var syntax in attributeGroup)
                        {
                            var destinationType = CheckArgument(syntax.ArgumentList!.Arguments[0]);
                            _constructorArgType = attributeGroup.Key > 1
                                ? CheckArgument(syntax.ArgumentList.Arguments[1])
                                : null;
                            _sourceName = destinationType + ".g.cs";
                            node = node
                                .WithIdentifier(destinationType)
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                    SyntaxFactory.Token(SyntaxKind.PartialKeyword));
                            result = result is null ? node : result.AddMembers(node);
                        }
                        break;
                    }
                }
            }
        }
        
        return result;
    }
    
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var result = VisitTypeDeclaration(node);
        return result is null ? null : base.VisitClassDeclaration((ClassDeclarationSyntax)result);
    }
    public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
    {
        var result = VisitTypeDeclaration(node);
        return result is null ? null : base.VisitStructDeclaration((StructDeclarationSyntax)result);
    }

    public override SyntaxNode? VisitAttributeList(AttributeListSyntax node)
    {
        // if they're all Jankneric Attributes then we remove the whole list
        if (node.Attributes.All(attr => attr.Name.ToString() == "Jankneric"))
            return null;
        
        // otherwise we remove only the Jankneric attributes from the list
        foreach (var attribute in node.Attributes.Where(attr => attr.Name.ToString() == "Jankneric"))
        {
            var result = node.RemoveNode(attribute, SyntaxRemoveOptions.KeepNoTrivia);
            if (result is null)
                return null; //TODO error??
            node = result;
        }

        // continue the recursion
        return base.VisitAttributeList(node);
    }
}