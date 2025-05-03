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
        public void MultipleTest()
        {
            // Create the 'input' compilation that the generator will act on
            var inputCompilation = CreateCompilation(
                """
                using Janknerics;
                namespace TestCode;
                
                public partial class GeneratedClass1;
                public partial class GeneratedClass2;
                
                [Jankneric(typeof(GeneratedClass1))]
                [Jankneric(typeof(GeneratedClass2))]
                class TemplateClass
                {
                    [Jankneric(typeof(GeneratedClass1), typeof(double))]
                    public float P1 = 0;
                    [Jankneric(typeof(GeneratedClass2), typeof(int))]
                    public float P2 = 0;
                };
                """);

            var runResult = Compile(inputCompilation, out var outputCompilation, out var diagnostics);

            Debug.Assert(runResult.GeneratedTrees.Length == 2);
            Debug.Assert(runResult.Diagnostics.IsEmpty);
            Debug.Assert(runResult.Results.All(r => r.Exception is null));
        }

    }
}