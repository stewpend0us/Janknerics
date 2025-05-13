using System.Diagnostics;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Janknerics;

internal class Rewriter : CSharpSyntaxVisitor<IEnumerable<TypeDeclarationSyntax>>
{

    //private static readonly DiagnosticDescriptor RuleTest = new DiagnosticDescriptor("JANK0000","title","format","category",DiagnosticSeverity.Warning, true);

    //public Action<Diagnostic>? ReportDiagnostic { get; set; }
    
    public static readonly SyntaxToken ConstructorArgName = SyntaxFactory.Identifier("source");

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
        template = ExtractJank(template, ref spec);
        // remove Jankneric attributes from each MemberDeclaration
        foreach (var member in template.Members)
        {
            var cleanMember = ExtractJank(member, ref spec);
        //    foreach (var (target, args) in memberData)
        //    {
        //        if (!spec.ContainsKey(target))
        //            spec.Add(target, new GeneratorSpec());
        //        spec[target].Members.Add(new(cleanMember, args));
        //    }
        }

        template = template.WithMembers(SyntaxFactory.List(spec.Values.SelectMany(s => s.Members.Select(m => m.Template))));
        
        // finally loop through the dictionary and actually create the output
        foreach (var kv in spec)
            yield return Rewrite(template, kv.Key, kv.Value);
    }

    private T ExtractJank<T>(T template, ref Dictionary<TypeSyntax, GeneratorSpec?> spec) where T : MemberDeclarationSyntax
    {
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
        template = (T)template.WithAttributeLists(SyntaxFactory.List(keptAttributes));
        
        foreach (var jank in jankAttributes.SelectMany(attr => attr.Attributes))
        {
            // just need to fill out the spec
            var target = ((TypeOfExpressionSyntax)jank.ArgumentList!.Arguments[0].Expression).Type;
            if (!spec.ContainsKey(target))
                spec.Add(target, new());
            switch (jank.Name.ToString())
            {
                case JanknericConstructorAttribute.Name:
                    spec[target]!.Constructor = new(SyntaxFactory.ParseTypeName(target.ToString()));
                    break;
                case JanknericAttribute.Name:
                {
                    var newTypeExpression =
                        GetAttributeNamedArgument(jank.ArgumentList.Arguments, nameof(JanknericAttribute.NewType)) as
                            TypeOfExpressionSyntax;
                    var conversionMethod =
                        GetAttributeNamedArgument(jank.ArgumentList.Arguments, nameof(JanknericAttribute.ConversionMethod))
                            is not MemberAccessExpressionSyntax conversionMethodExpression
                            ? ConversionMethod.Automatic
                            : (ConversionMethod)Enum.Parse(typeof(ConversionMethod),
                                conversionMethodExpression.Name.ToString());
                    var conversionFunctionName = GetAttributeNamedArgument(jank.ArgumentList.Arguments,
                        nameof(JanknericAttribute.ConversionFunctionName))?.ToString();

                    spec[target]?.Members.Add(new(template, newTypeExpression?.Type, conversionMethod, conversionFunctionName));
                    break;
                }
            }
        }
        return template;
    }

    private ExpressionSyntax? GetAttributeNamedArgument(SeparatedSyntaxList<AttributeArgumentSyntax> args, string argName)
    {
        return args.FirstOrDefault(a => a.NameEquals?.Name.ToString() == argName)?.Expression;
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
        List<TypeSyntax> originalType = [];
        List<MemberDeclarationSyntax> newMembers = [];
        foreach (var item in targetInfo.Members)
        {
            
            switch (item.Template)
            {
                case FieldDeclarationSyntax field:
                    newMembers.Add(item.NewType is null
                        ? field
                        : field.WithDeclaration(field.Declaration.WithType(item.NewType)));

                    originalType.Add(field.Declaration.Type);
                    break;
                case PropertyDeclarationSyntax property:
                    newMembers.Add(item.NewType is null
                        ? property
                        : property.WithType(item.NewType));
                    
                    originalType.Add(property.Type);
                    break;
            }
        }
        
        if (targetInfo.Constructor is not null)
        {
            var modifiers = SyntaxFactory.TokenList([SyntaxFactory.Token(SyntaxKind.PublicKeyword)]);
            var arg = SyntaxFactory.Parameter(ConstructorArgName)
                .WithType(SyntaxFactory.ParseTypeName(template.Identifier.ToString()));
            var args = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList([arg]));
            var constructedType = SyntaxFactory.Identifier(targetType.ToString());
            var body = SyntaxFactory.Block(ConstructorStatements(originalType, ref newMembers, targetInfo.Members));
            var constructor = SyntaxFactory.ConstructorDeclaration([], modifiers, constructedType, args, null, body);
            newMembers.Add(constructor);
        }
        
        List<SyntaxToken> modifiersToAdd = [];
        //AddIfAbsent(SyntaxKind.PublicKeyword);
        AddIfAbsent(SyntaxKind.PartialKeyword);
        
        return template
            .WithIdentifier(SyntaxFactory.Identifier(targetType.ToString()))
            .WithModifiers(SyntaxFactory.TokenList(modifiersToAdd.ToArray()))
            .WithMembers(SyntaxFactory.List(newMembers));
        
        void AddIfAbsent(SyntaxKind kind)
        {
            if (!template.Modifiers.Any(m => m.IsKind(kind)))
                modifiersToAdd.Add(SyntaxFactory.Token(kind));
        }
    }

    private static SyntaxList<StatementSyntax> ConstructorStatements(List<TypeSyntax> originalType, ref List<MemberDeclarationSyntax> members, List<GeneratorMember> memberInfo)
    {
        Debug.Assert(originalType.Count == members.Count);
        
        List<StatementSyntax> statements = [];

        for (var i = 0; i < members.Count; i++)
        {
            StatementSyntax? statement;
            switch (members[i])
            {
                case PropertyDeclarationSyntax property:
                    statement = ConstructorAssign(property.Identifier, property.Type, originalType[i], memberInfo[i]);
                    if (statement is not EmptyStatementSyntax)
                        members[i] = property.WithInitializer(null).WithTrailingTrivia(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));
                    break;
                case FieldDeclarationSyntax field:
                    statement = ConstructorAssign(field.Declaration.Variables.First().Identifier, field.Declaration.Type, originalType[i], memberInfo[i]);
                    if (statement is not EmptyStatementSyntax)
                        members[i] = field.WithDeclaration(field.Declaration.WithVariables(SyntaxFactory.SeparatedList([field.Declaration.Variables.First().WithInitializer(null)])));
                    break;
                default:
                    statement = SyntaxFactory.EmptyStatement();
                    break;
            }

            statements.Add(statement);
        }
        
        return SyntaxFactory.List(statements);
    }

    private static StatementSyntax ConstructorAssign(SyntaxToken name, TypeSyntax leftType, TypeSyntax rightType, GeneratorMember conversionMethod)
    {
        switch (conversionMethod.Method)
        {
            case ConversionMethod.Automatic:
                return ConstructorAssignAuto(name, leftType, rightType, conversionMethod);
            case ConversionMethod.Assign:
                return SyntaxFactory.ParseStatement($"{name} = {ConstructorArgName}.{name};");
            case ConversionMethod.Cast:
                return SyntaxFactory.ParseStatement($"{name} = ({leftType}){ConstructorArgName}.{name};");
            case ConversionMethod.Construct:
                return SyntaxFactory.ParseStatement($"{name} = new ({ConstructorArgName}.{name});");
            case ConversionMethod.Specified:
                if (conversionMethod.CustomMethodName is not null)
                    return SyntaxFactory.ParseStatement($"{name} = {conversionMethod.CustomMethodName}({ConstructorArgName}.{name});");
                // TODO this is an error
                break;
            default:
                // TODO give a warning!
                break;
        }
        
        return SyntaxFactory.EmptyStatement();
    }

    private static StatementSyntax ConstructorAssignAuto(SyntaxToken name, TypeSyntax leftType, TypeSyntax rightType, GeneratorMember conversionMethod)
    {
        
        if (conversionMethod.CustomMethodName is not null)
        {
            conversionMethod.Method = ConversionMethod.Specified;
            return ConstructorAssign(name, leftType, rightType, conversionMethod);
        }

        if (leftType.IsEquivalentTo(rightType))
        {
            conversionMethod.Method = ConversionMethod.Assign;
            return ConstructorAssign(name, leftType, rightType, conversionMethod);
        }
        
        if (leftType is PredefinedTypeSyntax left)
        {
            switch (left.Keyword.Kind())
            {
                case SyntaxKind.StringKeyword: 
                    return SyntaxFactory.ParseStatement($"{name} = {ConstructorArgName}.{name}.ToString();");
                case SyntaxKind.ObjectKeyword:
                    conversionMethod.Method = ConversionMethod.Assign;
                    return ConstructorAssign(name, leftType, rightType, conversionMethod);
            }
            if (rightType is PredefinedTypeSyntax right)
            {
                // TODO surely we could do better?
                conversionMethod.Method = ConversionMethod.Cast;
                return ConstructorAssign(name, leftType, rightType, conversionMethod);
            }
        }
        
        // TODO error they should specify a conversion method or function
        return SyntaxFactory.EmptyStatement();
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