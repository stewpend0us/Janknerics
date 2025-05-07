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

    //private static readonly DiagnosticDescriptor RuleTest = new DiagnosticDescriptor("JANK0000","title","format","category",DiagnosticSeverity.Warning, true);
    private static readonly DiagnosticDescriptor RuleNotAType = new DiagnosticDescriptor("JANK0001","JanknericAttribute Arguments","All arguments of Jankneric Attribute must be of type 'Type'","JanknericAttribute", DiagnosticSeverity.Error, true);
    
        
    public Action<Diagnostic>? ReportDiagnostic { get; set; }

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

    private TypeSyntax? CheckArgument(AttributeArgumentSyntax argument)
    {
        if (argument.Expression is TypeOfExpressionSyntax typeOfExpression)
            return typeOfExpression.Type;
        ReportDiagnostic?.Invoke(Diagnostic.Create(RuleNotAType, Location.None));
        return null;
    }

    private SyntaxList<TypeSyntax> CheckArguments(SeparatedSyntaxList<AttributeArgumentSyntax> arguments) =>
        new (arguments.Select(CheckArgument).OfType<TypeSyntax>());

    /// <summary>
    /// if the attribute is a Jankneric add it to the dictionary and return null
    /// otherwise keep it
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="janknericAttributes"></param>
    /// <returns></returns>
    /// <exception cref="CustomAttributeFormatException"></exception>
    private AttributeSyntax? Pop(AttributeSyntax attribute, ref Dictionary<TypeSyntax, SyntaxList<TypeSyntax>> janknericAttributes)
    {
        if (attribute.Name.ToString() != "Jankneric")
            return attribute;
        
        // must have arguments
        if (attribute.ArgumentList is null || attribute.ArgumentList.Arguments.Count == 0)
            throw new CustomAttributeFormatException("JanknericAttribute has no arguments");
        
        // verify the attribute arguments have the right type
        // and organize them into a dictionary where the first argumetn is the key and the remaining arguments are the value
        var attributeArguments = attribute.ArgumentList.Arguments;
        var key = CheckArgument(attributeArguments[0]);
        if (janknericAttributes.ContainsKey(key))
            throw new CustomAttributeFormatException($"Multiple JanknericAttribute with key '{key}' on the same member");
        janknericAttributes[key] = CheckArguments(attributeArguments.RemoveAt(0));
        return null; // consume the attribute
    }
    
    /// <summary>
    /// remove any Jankneric attributes from the list
    /// </summary>
    /// <param name="list"></param>
    /// <param name="janknericAttributes"></param>
    /// <returns></returns>
    private AttributeListSyntax Pop(AttributeListSyntax list, ref Dictionary<TypeSyntax, SyntaxList<TypeSyntax>> janknericAttributes)
    {
        var result = SyntaxFactory.AttributeList();
        foreach (var attribute in list.Attributes)
            if (Pop(attribute, ref janknericAttributes) is { } newAttribute)
                result = result.AddAttributes(newAttribute);
        return result;
    }
    
    /// <summary>
    /// compare TypeSyntax using their string representation
    /// </summary>
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
            return node; // 'passthrough' case
        if (destinationType.Count != 1)
            throw new CustomAttributeFormatException("Field declaration must have only one type after the key");
        var type = destinationType[0];
        if (!type.IsKind(SyntaxKind.PredefinedType))
        {
            // TODO do need to do something smarter with reference types?
            // the attributes on the expected class have a nullable but the generated type doesn't for the CustomTypeTestClasses
        }
        return node switch
        {
            FieldDeclarationSyntax field => field.WithDeclaration(field.Declaration.WithType(type)),
            PropertyDeclarationSyntax property => property.WithType(type),
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

        List<SyntaxToken> modifiersToAdd = [];
        AddIfAbsent(SyntaxKind.PublicKeyword);
        AddIfAbsent(SyntaxKind.PartialKeyword);
        
        return node
            .WithMembers(members)
            .WithIdentifier(SyntaxFactory.Identifier(destinationType.ToString()))
            .AddModifiers(modifiersToAdd.ToArray());

        void AddIfAbsent(SyntaxKind kind)
        {
            if (!node.Modifiers.Any(m => m.IsKind(kind)))
                modifiersToAdd.Add(SyntaxFactory.Token(kind));
        }
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