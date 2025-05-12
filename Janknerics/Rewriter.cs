using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Janknerics;

internal class Rewriter : CSharpSyntaxVisitor<IEnumerable<TypeDeclarationSyntax>>
{

    //private static readonly DiagnosticDescriptor RuleTest = new DiagnosticDescriptor("JANK0000","title","format","category",DiagnosticSeverity.Warning, true);

    //public Action<Diagnostic>? ReportDiagnostic { get; set; }

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
    public override IEnumerable<TypeDeclarationSyntax> DefaultVisit(SyntaxNode node)
    {
        if (node is BaseNamespaceDeclarationSyntax ns)
            foreach (var m in ns.Members)
                if (base.Visit(m) is { } visited)
                    foreach (var v in visited)
                        yield return v;
    }

    /// <summary>
    /// visit any TypeDeclarationSyntax we encounter
    /// if it contains any Jankneric attributes then split it into the corresponding number of output types
    /// </summary>
    /// <param name="template"></param>
    /// <returns></returns>
    private IEnumerable<TypeDeclarationSyntax> VisitTypeDeclaration(TypeDeclarationSyntax template)
    {
        Dictionary<TypeSyntax, GeneratorSpec> spec = new (TypeSyntaxComparer);
        
        // remove Jankneric attributes from the TypeDeclaration
        template = ExtractJank(template, out var constructorData);
        foreach (var (target, arg) in constructorData)
        {
            if (!spec.ContainsKey(target))
                spec.Add(target, new());
            Debug.Assert(spec[target].Constructor is null, "constructor is not null");
            spec[target].Constructor = arg;
        }
        
        // remove Jankneric attributes from each MemberDeclaration
        foreach (var member in template.Members)
        {
            var cleanMember = ExtractJank(member, out var memberData);
            foreach (var (target, args) in memberData)
            {
                if (!spec.ContainsKey(target))
                    spec.Add(target, new GeneratorSpec());
                spec[target].Members.Add(new(cleanMember, args));
            }
        }

        template = template.WithMembers(SyntaxFactory.List(spec.Values.SelectMany(s => s.Members.Select(m => m.Template))));
        
        // finally loop through the dictionary and actually create the output
        foreach (var kv in spec)
            yield return Rewrite(template, kv.Key, kv.Value);
    }

    // TODO targetAndArgs here doesn't distinquish between attributes...but it needs to.
    private T ExtractJank<T>(T template, out List<(TypeSyntax, TypeSyntax?)> targetAndArgs) where T : MemberDeclarationSyntax
    {
        targetAndArgs = [];
        List<AttributeListSyntax> keptAttributes = [];
        List<AttributeListSyntax> jankAttributes = [];
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
            if (jank.ArgumentList!.Arguments
                    .FirstOrDefault(a => a.NameEquals?.Name.ToString() == nameof(JanknericAttribute.ReplacementType))
                    ?.Expression is TypeOfExpressionSyntax value)
                arg = value.Type;
            targetAndArgs.Add((target, arg));
        }
        return (T)template.WithAttributeLists(SyntaxFactory.List(keptAttributes));
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
        List<AttributeSyntax> cleanList = [];
        List<AttributeSyntax> dirtyList = [];
        foreach (var attr in source.Attributes)
        {
            var name = attr.Name.ToString();
            if (name.Equals(JanknericAttribute.Name) || name.Equals(JanknericConstructorAttribute.Name))
                dirtyList.Add(attr);
            else
                cleanList.Add(attr);
        }
        clean = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(cleanList));
        dirty = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(dirtyList));
    }

    private TypeDeclarationSyntax Rewrite(TypeDeclarationSyntax template, TypeSyntax targetType, GeneratorSpec targetInfo)
    {
        List<MemberDeclarationSyntax> members = [];
        if (targetInfo.Constructor is not null)
        {
            var arg = SyntaxFactory.Parameter(SyntaxFactory.Identifier(targetInfo.Constructor.ToString()));
            var args = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList([arg]));
            var constructor = SyntaxFactory.ConstructorDeclaration(targetType.ToString()).WithParameterList(args);
            members.Add(constructor);
        }
        foreach (var item in targetInfo.Members)
        {
            
            switch (item.Template)
            {
                case FieldDeclarationSyntax field:
                    members.Add(item.NewType is null
                        ? field
                        : field.WithDeclaration(field.Declaration.WithType(item.NewType)));
                    break;
                case PropertyDeclarationSyntax property:
                    members.Add(item.NewType is null
                        ? property
                        : property.WithType(item.NewType));
                    break;
            }
        }
        List<SyntaxToken> modifiersToAdd = [];
        AddIfAbsent(SyntaxKind.PublicKeyword);
        AddIfAbsent(SyntaxKind.PartialKeyword);
        
        return template
            .WithIdentifier(SyntaxFactory.Identifier(targetType.ToString()))
            .WithModifiers(SyntaxFactory.TokenList(modifiersToAdd.ToArray()))
            .WithMembers(SyntaxFactory.List(members));
        
        void AddIfAbsent(SyntaxKind kind)
        {
            if (!template.Modifiers.Any(m => m.IsKind(kind)))
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