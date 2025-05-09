using System.Diagnostics;
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
    private IEnumerable<TypeDeclarationSyntax> VisitTypeDeclaration(TypeDeclarationSyntax templateType)
    {
        List<(TypeSyntax, TypeSyntax?)> constructors = [];
        // remove Jankneric attributes from the TypeDeclaration
        templateType = ExtractJank(templateType, ref constructors);
        
        // remove Jankneric attributes from each MemberDeclaration
        List<(TypeSyntax, TypeSyntax?)> members = [];
        SyntaxList<MemberDeclarationSyntax> cleanMembers = [];
        foreach (var member in templateType.Members)
            cleanMembers.Add(ExtractJank(member, ref members));
        templateType = templateType.WithMembers(cleanMembers);
        // the template is now jank free
        // and we've collected all the data from the attributes in 'constructors' and 'members'
        
        // fill the dictionary (just groups things by generated type for convenience)
        Dictionary<TypeSyntax, GeneratedClassSpec> spec = new (TypeSyntaxComparer);
        foreach (var (target, arg) in constructors)
            if (arg is not null)
                spec[target].Constructor.Add(arg);
        foreach (var (target, arg) in members)
            spec[target].Member.Add(arg);

        // finally loop through the dictionary and actually create the output
        foreach (var kv in spec)
        {
            var targetType = kv.Key;
            var info = kv.Value;
            yield return Rewrite(templateType, targetType, info);
        }
    }

    private T ExtractJank<T>(T template, ref List<(TypeSyntax, TypeSyntax?)> targetAndArgs) where T : MemberDeclarationSyntax
    {
        SyntaxList<AttributeListSyntax> keptAttributes = [];
        SyntaxList<AttributeListSyntax> jankAttributes = [];
        foreach (var attributeList in template.AttributeLists)
        {
            ExtractJank(attributeList, out var clean, out var dirty);
            if (clean.Attributes.Any())
                keptAttributes.Add(clean);
            if (dirty.Attributes.Any())
                jankAttributes.Add(dirty);
        }

        foreach (var jank in jankAttributes.SelectMany(attr => attr.Attributes))
        {
            // already verified so we should be good to go
            // just need to fill out the spec
            var target = ((TypeOfExpressionSyntax)jank.ArgumentList!.Arguments[0].Expression).Type;
            TypeSyntax? arg = null;
            if (jank.ArgumentList!.Arguments.Count == 2)
                arg = ((TypeOfExpressionSyntax)jank.ArgumentList!.Arguments[1].Expression).Type;
            targetAndArgs.Add((target, arg));
        }
        return (T)template.WithAttributeLists(keptAttributes);
    }

    /// <summary>
    /// remove any Jankneric attributes from the list
    /// </summary>
    /// <param name="source"></param>
    /// <param name="clean"></param>
    /// <param name="dirty"></param>
    /// <returns></returns>
    private void ExtractJank(AttributeListSyntax source, out AttributeListSyntax clean, out AttributeListSyntax dirty)
    {
        var cleanList = SyntaxFactory.SeparatedList<AttributeSyntax>();
        var dirtyList = SyntaxFactory.SeparatedList<AttributeSyntax>();
        foreach (var attr in source.Attributes)
        {
            if (attr.ToString().Equals("Jankneric"))
            {
                if (VerifyJank(attr))
                    dirtyList.Add(attr);
            }
            else
                cleanList.Add(attr);
        }
        clean = SyntaxFactory.AttributeList(cleanList);
        dirty = SyntaxFactory.AttributeList(dirtyList);
    }

    private bool VerifyJank(AttributeSyntax jank)
    {
        // must have arguments
        if (jank.ArgumentList is null || jank.ArgumentList.Arguments.Count == 0)
        {
            ReportDiagnostic?.Invoke(Diagnostic.Create(RuleNoArguments, Location.None));
            return false;
        }
        
        if (jank.ArgumentList.Arguments.Count > 2)
        {
            // TODO report
            return false;
        }
        
        foreach (var arg in jank.ArgumentList.Arguments)
        {
            if (arg.Expression is TypeOfExpressionSyntax typeOfExpression)
                return true;
            ReportDiagnostic?.Invoke(Diagnostic.Create(RuleNotAType, Location.None));
            return false;
        }
        return true;
    }
    
    private TypeDeclarationSyntax Rewrite(TypeDeclarationSyntax templateType, TypeSyntax targetType, GeneratedClassSpec info)
    {
        Debug.Assert(templateType.Members.Count == info.Member.Count);
        // TODO fill this out
        throw new NotImplementedException();
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
        foreach (var member in node.Members)
        {
            var newMember = ExtractTypeJank(member, out var janknericAttributes);
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