using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Janknerics;

[Generator]
public class JanknericsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var namespaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is BaseNamespaceDeclarationSyntax,
                transform: static (ctx, _) => (BaseNamespaceDeclarationSyntax)ctx.Node
            );

        //var compilationAndNamespaces= context.CompilationProvider.Combine(namespaceDeclarations.Collect());

        context.RegisterSourceOutput(namespaceDeclarations.Collect(), (spc, source) => Execute(source, spc));
    }


    private readonly Rewriter _rewriter = new();

    private void Execute(IReadOnlyList<BaseNamespaceDeclarationSyntax> namespaces, SourceProductionContext context)
    {
        //_rewriter.ReportDiagnostic = context.ReportDiagnostic;
        foreach (var ns in namespaces)
        {
            //bool hasNullableEnabled = ns
            //    .DescendantTrivia()
            //    .Any(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) && 
            //              t.ToString().Contains("#nullable enable"));
            if (_rewriter.Visit(ns) is { } templates)
            {
                foreach (var template in templates)
                {
                    if (template is not null)
                    {
                        var src = ns
                            .WithMembers(new(template))
                            .NormalizeWhitespace()
                            .ToString();

                        var id = Path.Combine(ns.Name.ToString(), template.Identifier + ".g.cs");
                        context.AddSource(id, src);
                    }
                }
            }
        }
    }
}