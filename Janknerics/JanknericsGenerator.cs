using Microsoft.CodeAnalysis;
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

        private void Execute(Compilation compilation, IReadOnlyList<BaseNamespaceDeclarationSyntax> namespaces, SourceProductionContext context)
        {
            foreach (var ns in namespaces)
            {
                JanknericsRewriter.Rewrite(ns,context.AddSource);
            }
        }
    }
}
