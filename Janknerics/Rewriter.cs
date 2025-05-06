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
public class Rewriter : CSharpSyntaxVisitor<IEnumerable<TypeDeclarationSyntax>>
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
                if (base.Visit(m) is { } visited)
                    foreach (var v in visited)
                        yield return v;

        if (base.DefaultVisit(node) is { } result)
            foreach (var r in result)
                yield return r;
    }

    /// <summary>
    /// visit any TypeDeclarationSyntax we encounter
    /// if it contains any Jankneric attributes then split it into the corresponding number of output types
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private IEnumerable<TypeDeclarationSyntax> VisitTypeDeclaration(TypeDeclarationSyntax node)
    {
        node = Pop(node, out var janknericAttributes);

        foreach (var kv in janknericAttributes)
            yield return Rewrite(node, kv.Key, kv.Value);
    }

    private TypeSyntax CheckArgument(AttributeArgumentSyntax argument)
    {
            if (argument.Expression is not TypeOfExpressionSyntax typeOfExpression)
                throw new CustomAttributeFormatException(
                    "All arguments of Jankneric attribute must be 'typeof' expression");
            return typeOfExpression.Type;
    }

    private SyntaxList<TypeSyntax> CheckArguments(SeparatedSyntaxList<AttributeArgumentSyntax> arguments) =>
        new (arguments.Select(CheckArgument));

    private AttributeSyntax? Pop(AttributeSyntax node, ref Dictionary<TypeSyntax, SyntaxList<TypeSyntax>> janknericAttributes)
    {
        if (node.Name.ToString() != "Jankneric")
            return node;
        
        // must have arguments
        if (node.ArgumentList is null || node.ArgumentList.Arguments.Count == 0)
            throw new CustomAttributeFormatException("JanknericAttribute has no arguments");
        
        // verify the attribute arguments have the right type
        // and organize them into a dictionary where the first argumetn is the key and the remaining arguments are the value
        var attributeArguments = node.ArgumentList.Arguments;
        var key = CheckArgument(attributeArguments[0]);
        if (janknericAttributes.ContainsKey(key))
            throw new CustomAttributeFormatException($"Multiple JanknericAttribute with key '{key}' on the same member");
        janknericAttributes[key] = CheckArguments(attributeArguments.RemoveAt(0));
        return null; // consume the attribute
    }
    
    private AttributeListSyntax Pop(AttributeListSyntax node, ref Dictionary<TypeSyntax, SyntaxList<TypeSyntax>> janknericAttributes)
    {
        var result = SyntaxFactory.AttributeList();
        foreach (var attribute in node.Attributes)
            if (Pop(attribute, ref janknericAttributes) is { } newAttribute)
                result = result.AddAttributes(newAttribute);
        return result;
    }
    
    private class CompareTypeSyntax : IEqualityComparer<TypeSyntax>
    {
        public bool Equals(TypeSyntax x, TypeSyntax y) => x.ToString() == y.ToString();
        public int GetHashCode(TypeSyntax obj) => obj.ToString().GetHashCode();
    }

    private static readonly CompareTypeSyntax TypeSyntaxComparer = new();

    /// <summary>
    /// Remove any Jankneric attributes from the node
    /// </summary>
    /// <param name="node"></param>
    /// <param name="janknericAttributes"></param>
    /// <returns></returns>
    private T Pop<T>(T node, out Dictionary<TypeSyntax, SyntaxList<TypeSyntax>> janknericAttributes) where T : MemberDeclarationSyntax
    {
        janknericAttributes = new (TypeSyntaxComparer);
        SyntaxList<AttributeListSyntax> attributes = [];
        foreach (var attributeList in node.AttributeLists)
        {
            var newAttributes = Pop(attributeList, ref janknericAttributes);
            if (newAttributes.Attributes.Any())
                attributes.Add(newAttributes);
        }
        return (T)node.WithAttributeLists(attributes);
    }

    private static MemberDeclarationSyntax Rewrite(MemberDeclarationSyntax node, SyntaxList<TypeSyntax> destinationType)
    {
        if (destinationType.Count == 0)
            return node;
        if (destinationType.Count != 1)
            throw new CustomAttributeFormatException("Field declaration must have only one type after the key");
        return node switch
        {
            FieldDeclarationSyntax field => field.WithDeclaration(field.Declaration.WithType(destinationType[0])),
            PropertyDeclarationSyntax property => property.WithType(destinationType[0]),
            _ => node
        };
    }
    
    /// <summary>
    /// iterate through and re-write or remove each member of the Type
    /// </summary>
    /// <param name="node"></param>
    /// <param name="destinationType"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private TypeDeclarationSyntax Rewrite(TypeDeclarationSyntax node, TypeSyntax destinationType, SyntaxList<TypeSyntax> args)
    {
        SyntaxList<MemberDeclarationSyntax> members = [];
        foreach (var member in node.Members)
        {
            var newMember = Pop(member, out var janknericAttributes);
            if (!janknericAttributes.TryGetValue(destinationType, out var attributes))
                continue;
            members = members.Add(Rewrite(newMember, attributes));
        }

        return node
            .WithMembers(members)
            .WithIdentifier(SyntaxFactory.Identifier(destinationType.ToString()))
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
}