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
public class Rewriter : CSharpSyntaxVisitor<IEnumerable<TypeDeclarationSyntax?>>
{

    //private static readonly DiagnosticDescriptor RuleTest = new DiagnosticDescriptor("JANK0000","title","format","category",DiagnosticSeverity.Warning, true);
    private static readonly DiagnosticDescriptor RuleNotAType = new DiagnosticDescriptor("JANK0001",$"{nameof(JanknericAttribute)} Arguments with wrong type",$"All arguments of {nameof(JanknericAttribute)} must be of type 'Type'","JanknericAttribute", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor RuleNoArguments = new DiagnosticDescriptor("JANK0002",$"{nameof(JanknericAttribute)} with no arguments",$"{nameof(JanknericAttribute)} must have at least one argument","JanknericAttribute", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor RuleDuplicateKey = new DiagnosticDescriptor("JANK0003",$"Duplicate {nameof(JanknericAttribute)}",$"First argument of {nameof(JanknericAttribute)} must be unique on a given type","JanknericAttribute", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor RuleTwoArguments = new DiagnosticDescriptor("JANK0004",$"{nameof(JanknericAttribute)} with more than two arguments",$"{nameof(JanknericAttribute)} can have at most two arguments","JanknericAttribute", DiagnosticSeverity.Error, true);
        
    public static Action<Diagnostic>? ReportDiagnostic { get; set; }

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
    /// visit all members of any BaseNamespaceDeclarationSyntax we encounter
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public override IEnumerable<TypeDeclarationSyntax?> DefaultVisit(SyntaxNode node)
    {
        if (node is BaseNamespaceDeclarationSyntax ns)
            foreach (var m in ns.Members)
                if (base.Visit(m) is { } visited)
                    foreach (var v in visited)
                        yield return v;

        //if (base.DefaultVisit(node) is { } result)
        //    foreach (var r in result)
        //        yield return r;
    }

    /// <summary>
    /// visit any TypeDeclarationSyntax we encounter
    /// if it contains any Jankneric attributes then split it into the corresponding number of output types
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private IEnumerable<TypeDeclarationSyntax?> VisitTypeDeclaration(TypeDeclarationSyntax node)
    {
        node = ExtractJank(node, out var janknericAttributes);
        foreach (var kv in janknericAttributes)
            yield return Rewrite(node, kv);
    }

    /// <summary>
    /// Remove any Jankneric attributes from the node
    /// </summary>
    /// <param name="node"></param>
    /// <param name="janknericAttributes"></param>
    /// <returns></returns>
    private T ExtractJank<T>(T node, out Dictionary<TypeSyntax, GeneratedClassSpec> janknericAttributes) where T : MemberDeclarationSyntax
    {
        janknericAttributes = new (TypeSyntaxComparer);
        SyntaxList<AttributeListSyntax> attributes = [];
        foreach (var attributeList in node.AttributeLists)
        {
            var newAttributes = ExtractJank(attributeList, ref janknericAttributes);
            if (newAttributes.Attributes.Any())
                attributes.Add(newAttributes);
        }
        return (T)node.WithAttributeLists(attributes);
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
    private AttributeSyntax? ExtractJank(AttributeSyntax attribute, ref Dictionary<TypeSyntax, GeneratedClassSpec> janknericAttributes)
    {
        if (attribute.Name.ToString() != "Jankneric")
            return attribute;
        
        // must have arguments
        if (attribute.ArgumentList is null || attribute.ArgumentList.Arguments.Count == 0)
        {
            ReportDiagnostic?.Invoke(Diagnostic.Create(RuleNoArguments, Location.None));
            return attribute; // leave the error in the generated code
        }

        // verify the attribute arguments have the right type
        // and organize them into a dictionary where the first argumetn is the key and the remaining arguments are the value
        var attributeArguments = attribute.ArgumentList.Arguments;
        var key = CheckArgument(attributeArguments[0]);
        if (janknericAttributes.ContainsKey(key))
        {
            ReportDiagnostic?.Invoke(Diagnostic.Create(RuleDuplicateKey, Location.None));
            return attribute; // leave the error in the generated code
        }

        janknericAttributes[key] = CheckArguments(attributeArguments.RemoveAt(0));
        return null; // consume the attribute
    }
    
    /// <summary>
    /// remove any Jankneric attributes from the list
    /// </summary>
    /// <param name="list"></param>
    /// <param name="janknericAttributes"></param>
    /// <returns></returns>
    private AttributeListSyntax ExtractJank(AttributeListSyntax list, ref Dictionary<TypeSyntax, GeneratedClassSpec> janknericAttributes)
    {
        var result = SyntaxFactory.AttributeList();
        foreach (var attribute in list.Attributes)
            if (ExtractJank(attribute, ref janknericAttributes) is { } newAttribute)
                result = result.AddAttributes(newAttribute);
        return result;
    }
    
    private MemberDeclarationSyntax Rewrite(MemberDeclarationSyntax node, SyntaxList<TypeSyntax> destinationType)
    {
        if (destinationType.Count == 0)
            return node; // 'passthrough' case
        if (destinationType.Count != 1)
        {
            ReportDiagnostic?.Invoke(Diagnostic.Create(RuleTwoArguments, Location.None));
            return node;
        }
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
    private TypeDeclarationSyntax Rewrite(TypeDeclarationSyntax node, KeyValuePair<TypeSyntax, GeneratedClassSpec> entry)
    {
        SyntaxList<MemberDeclarationSyntax> newMembers = [];
        foreach (var member in node.Members)
        {
            Dictionary<TypeSyntax, GeneratedClassSpec> janknericAttributes;// = new(TypeSyntaxComparer);
            var newMember = ExtractJank(member, out janknericAttributes);
            if (!janknericAttributes.TryGetValue(destinationType, out var attributes))
                continue;
            newMembers = newMembers.Add(Rewrite(newMember, attributes));
        }

        List<SyntaxToken> modifiersToAdd = [];
        AddIfAbsent(SyntaxKind.PublicKeyword);
        AddIfAbsent(SyntaxKind.PartialKeyword);
        
        return node
            .WithMembers(newMembers)
            .WithIdentifier(SyntaxFactory.Identifier(destinationType.ToString()))
            .AddModifiers(modifiersToAdd.ToArray());

        void AddIfAbsent(SyntaxKind kind)
        {
            if (!node.Modifiers.Any(m => m.IsKind(kind)))
                modifiersToAdd.Add(SyntaxFactory.Token(kind));
        }
    }

    public override IEnumerable<TypeDeclarationSyntax?>? VisitClassDeclaration(ClassDeclarationSyntax node) =>
        VisitTypeDeclaration(node);   

    public override IEnumerable<TypeDeclarationSyntax?>? VisitStructDeclaration(StructDeclarationSyntax node) =>
        VisitTypeDeclaration(node);

    public override IEnumerable<TypeDeclarationSyntax?>? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) =>
        VisitTypeDeclaration(node);

    public override IEnumerable<TypeDeclarationSyntax?>? VisitRecordDeclaration(RecordDeclarationSyntax node) =>
        VisitTypeDeclaration(node);
}