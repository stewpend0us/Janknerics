using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Janknerics
{
    [Generator]
    public class JanknericsGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is BaseNamespaceDeclarationSyntax,
                    transform: static (ctx, _) => (BaseNamespaceDeclarationSyntax)ctx.Node
                )
                .Where(static m => m is not null);

            var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses, (spc, source) => Execute(source.Left, source.Right, spc));
        }

        
        private readonly Locator _locator = new ();
        
        private void Execute(Compilation compilation, IReadOnlyList<BaseNamespaceDeclarationSyntax> namespaces, SourceProductionContext context)
        {
            foreach (var ns in namespaces)
            {
                if (_locator.Visit(ns) is not { } templates)
                    continue;
                foreach (var t in templates)
                    if (t is not null)
                    {
                        var src = ns
                            .WithMembers(new(t))
                            .NormalizeWhitespace()
                            .ToString();

                        var id = t.Identifier + ".g.cs";
                        context.AddSource(id, src);
                    }
            }
        }
    }
}
