using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace Janknerics
{
    [Generator]
    public class JanknericsGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                    transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node
                )
                .Where(static m => m is not null);

            var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses, (spc, source) => Execute(source.Left, source.Right, spc));
        }

        private void Execute(Compilation compilation, IReadOnlyList<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            JanknericsRewriter rewriter = new JanknericsRewriter();
            
            foreach (var candidate in classes)
            {
                rewriter.Visit(candidate);
                context.AddSource(rewriter.SourceName, rewriter.Source);
                /*
                var attrs = candidate.AttributeLists
                    .Where(list => list.Attributes
                        .Where(attr => attr.Name.ToString() == "Jankneric")
                        .Where(attr => (attr.Parent as ClassDeclarationSyntax).Members
                            .Where(m => m.AttributeLists
                                .Where(attrr => attrr.Attributes
                                    .Where(attrrr => attrrr.Name.ToString() == "Jankneric")))));
                if (!attrs.Any())
                    continue;

                //if (viable(candidate))
                var model = compilation.GetSemanticModel(candidate.SyntaxTree);

                var classSymbol = model.GetDeclaredSymbol(candidate);
                if (classSymbol == null)
                    continue;

                var classLevelJanknericAttrs = classSymbol.GetAttributes()
                    .Where(attr => attr.AttributeClass?.ToDisplayString() == "Janknerics.JanknericAttribute")
                    .ToList();

                if (classLevelJanknericAttrs.Count == 0)
                    continue;

                var properties = candidate.Members
                    .OfType<PropertyDeclarationSyntax>()
                    .Concat<MemberDeclarationSyntax>(candidate.Members.OfType<FieldDeclarationSyntax>())
                    .ToList();

                var targets = new Dictionary<string, List<(string Name, string? Type, bool IsProperty)>>();

                foreach (var attr in classLevelJanknericAttrs)
                {
                    var targetClass = attr.ConstructorArguments[0].Value as INamedTypeSymbol;
                    if (targetClass == null)
                        continue;

                    var targetName = targetClass.ToDisplayString();
                    targets[targetName] = new List<(string, string?, bool)>();
                }

                foreach (var member in properties)
                {
                    var memberSymbol = model.GetDeclaredSymbol(member) as ISymbol;
                    var memberAttrs = memberSymbol?.GetAttributes()
                        .Where(attr => attr.AttributeClass?.ToDisplayString() == "Janknerics.JanknericAttribute")
                        .ToList();

                    foreach (var kvp in targets)
                    {
                        string targetName = kvp.Key;
                        var list = kvp.Value;

                        var matchingAttr = memberAttrs?.FirstOrDefault(attr =>
                        {
                            var target = attr.ConstructorArguments[0].Value as INamedTypeSymbol;
                            return target?.ToDisplayString() == targetName;
                        });

                        if (matchingAttr != null)
                        {
                            var repl = matchingAttr.ConstructorArguments.Length > 1 ? matchingAttr.ConstructorArguments[1].Value as INamedTypeSymbol : null;
                            list.Add((
                                memberSymbol.Name,
                                repl?.ToDisplayString(),
                                member is PropertyDeclarationSyntax
                            ));
                        }
                        else
                        {
                            // No override -> omit
                        }
                    }
                }

                foreach (var kvp in targets)
                {
                    var targetClassName = kvp.Key.Split('.').Last();
                    var sb = new StringBuilder();

                    sb.AppendLine("// Auto-generated by Janknerics");
                    sb.AppendLine("using System;");
                    sb.AppendLine();
                    sb.AppendLine($"partial class {targetClassName}");
                    sb.AppendLine("{");

                    foreach (var (name, typeName, isProperty) in kvp.Value)
                    {
                        if (typeName == null)
                            continue;

                        if (isProperty)
                            sb.AppendLine($"    public {typeName} {name} {{ get; set; }}");
                        else
                            sb.AppendLine($"    public {typeName} {name};");
                    }

                    sb.AppendLine("}");

                    context.AddSource($"{targetClassName}_jankneric.g.cs", sb.ToString());
                }
                
                */
            }
        }
    }
}
