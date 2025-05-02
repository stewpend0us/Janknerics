using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Janknerics;

public sealed class JanknericsRewriter : CSharpSyntaxRewriter
{

    private static readonly JanknericsRewriter Rewriter = new ();
    public static void Rewrite(TypeDeclarationSyntax node, Action<string, string> writer)
    {
        Rewriter._sourceName = string.Empty;
        
        var result = Rewriter.Visit(node);
        
        if (Rewriter._constructorArgType is not null)
            result = ApplyConstructor(result, (SyntaxToken)Rewriter._constructorArgType);
        
        if (Rewriter._sourceName != string.Empty)
            writer(Rewriter._sourceName, result.ToString());
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
        foreach (var attributeList in node.AttributeLists)
        {
            foreach (var attr in attributeList.Attributes)
            {
                if (attr.Name.ToString() != "Jankneric")
                    continue;
                if (attr.ArgumentList is null || attr.ArgumentList.Arguments.Count == 0)
                    throw new CustomAttributeFormatException("Jankneric attribute must have at least one argument");
                if (attr.ArgumentList.Arguments.Count > 2)
                    throw new CustomAttributeFormatException("Jankneric attribute must have at most two arguments");
                SyntaxToken destinationType;
                _constructorArgType = null;
                if (attr.ArgumentList.Arguments.Count == 1)
                {
                    destinationType = CheckArgument(attr.ArgumentList.Arguments[0]);
                }
                else
                {
                    destinationType = CheckArgument(attr.ArgumentList.Arguments[0]);
                    _constructorArgType = CheckArgument(attr.ArgumentList.Arguments[1]);
                }
                _sourceName = destinationType + ".g.cs";
                node = node
                    .WithIdentifier(destinationType);
            }
        }
        return _sourceName == string.Empty ? null : node;
    }

    public override SyntaxNode? VisitAttribute(AttributeSyntax node)
    {
        if (node.Name.ToString() != "Jankneric")
            return base.VisitAttribute(node);
        // TODO also remove the []
        return null;
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
}