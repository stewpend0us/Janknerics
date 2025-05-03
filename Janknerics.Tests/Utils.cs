using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Janknerics.Tests;

[TestClass]
public partial class JanknericsTests
{
    private static GeneratorDriverRunResult Compile(Compilation inputCompilation, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics)
    {
        
        var generator = new JanknericsGenerator();

        // Create the driver that will control the generation, passing in our generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the generation pass
        // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
        return driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out outputCompilation, out diagnostics).GetRunResult();
    }
    
    private static CSharpCompilation CreateCompilation(string source = "")
    {

        const string main = """
                            namespace TestCode;
                            public class Program
                            {
                                public static void Main(string[] args)
                                {
                                }
                            }
                            """;
        
        var basePath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        var netstandardPath = Path.Join(basePath, "netstandard.dll");
        var runtimePath = Path.Join(basePath, "System.Runtime.dll");
        
        var inputCompilation = CSharpCompilation.Create(
            "test_compilation",
            [
                CSharpSyntaxTree.ParseText(main),
                CSharpSyntaxTree.ParseText(source),
            ],
            [
                MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JanknericAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(netstandardPath),
                MetadataReference.CreateFromFile(runtimePath),
            ],
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        return inputCompilation;
    }
            
}