using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Reflection;

namespace Janknerics.Tests
{
    public partial class JanknericsTests
    {
        [TestMethod]
        public void BasicsTest()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(
                """
                using Janknerics;
                namespace MyCode
                {
                   public class Program
                   {
                       public static void Main(string[] args)
                       {
                       }
                   }
                   
                   partial class GeneratedClass;

                   [Jankneric(typeof(GeneratedClass))]
                   class TemplateClass
                   {
                       [Jankneric(typeof(GeneratedClass), typeof(double))]
                       public float P1 = 0;
                   }
                }
                """);

            // directly create an instance of the generator
            // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
            var generator = new JanknericsGenerator();

            // Create the driver that will control the generation, passing in our generator
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            // Run the generation pass
            // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            outputCompilation = outputCompilation.AddReferences(MetadataReference.CreateFromFile(typeof(JanknericAttribute).Assembly.Location));
            var basePath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var netstandardPath = Path.Join(basePath, "netstandard.dll");
            var runtimePath = Path.Join(basePath, "System.Runtime.dll");
            outputCompilation = outputCompilation.AddReferences(MetadataReference.CreateFromFile(netstandardPath));
            outputCompilation = outputCompilation.AddReferences(MetadataReference.CreateFromFile(runtimePath));
            
            // We can now assert things about the resulting compilation:
            Debug.Assert(diagnostics.IsEmpty); // there were no diagnostics created by the generators
            Debug.Assert(outputCompilation.SyntaxTrees.Count() == 2); // we have two syntax trees, the original 'user' provided one, and the one added by the generator
            Debug.Assert(outputCompilation.GetDiagnostics().IsEmpty); // verify the compilation with the added source has no diagnostics

            // Or we can look at the results directly:
            GeneratorDriverRunResult runResult = driver.GetRunResult();

            // The runResult contains the combined results of all generators passed to the driver
            Debug.Assert(runResult.GeneratedTrees.Length == 1);
            Debug.Assert(runResult.Diagnostics.IsEmpty);

            // Or you can access the individual results on a by-generator basis
            GeneratorRunResult generatorResult = runResult.Results[0];
            //Debug.Assert(generatorResult.Generator == generator);
            Debug.Assert(generatorResult.Diagnostics.IsEmpty);
            Debug.Assert(generatorResult.GeneratedSources.Length == 1);
            Debug.Assert(generatorResult.Exception is null);
        }

    }
}