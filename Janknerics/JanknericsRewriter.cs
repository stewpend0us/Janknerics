using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Janknerics;

public class NewClassSpec(
    TypeDeclarationSyntax sourceSyntax,
    TypeSyntax destinationType,
    TypeSyntax? constructorArgument)
{
    public TypeDeclarationSyntax Spec = sourceSyntax;
    public TypeSyntax Destination = destinationType;
    public TypeSyntax? ConstructorArgument = constructorArgument;
}

public class JanknericsRewriter : CSharpSyntaxVisitor
{

    private TypeSyntax CheckArgument(AttributeArgumentSyntax argument)
    {
        if (argument.Expression is not TypeOfExpressionSyntax typeOfExpression)
            throw new CustomAttributeFormatException(
                "All arguments of Jankneric attribute must be 'typeof' expression");
        return typeOfExpression.Type;
    }

    private void VisitTypeDeclaration(TypeDeclarationSyntax node)
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
                TypeSyntax destinationType;
                TypeSyntax? constructorArgType = null;
                if (attr.ArgumentList.Arguments.Count == 1)
                {
                    destinationType = CheckArgument(attr.ArgumentList.Arguments[0]);
                }
                else
                {
                    destinationType = CheckArgument(attr.ArgumentList.Arguments[0]);
                    constructorArgType = CheckArgument(attr.ArgumentList.Arguments[1]);
                }

                SourceName = destinationType.ToString() + ".g.cs";
                Source = "//hello!";
            }
        }
    }

    public string SourceName { get; private set; } = "";
    public string Source { get; private set; } = "";


    public override void VisitClassDeclaration(ClassDeclarationSyntax node) => VisitTypeDeclaration(node);
    public override void VisitStructDeclaration(StructDeclarationSyntax node) => VisitTypeDeclaration(node);
}