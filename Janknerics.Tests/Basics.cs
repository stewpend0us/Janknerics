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
            var inputCompilation = CreateCompilation(
                """
                using Janknerics;
                namespace TestCode;
                
                public partial class GeneratedClass;

                [Jankneric(typeof(GeneratedClass))]
                class TemplateClass
                {
                    [Jankneric(typeof(GeneratedClass), typeof(double))]
                    public float P1 = 0;
                    
                    [Jankneric(typeof(GeneratedClass), typeof(int))]
                    public double P2 {get; set;} = 4;
                    
                    [Jankneric(typeof(GeneratedClass))]
                    public string P3 { get; set; }  = "4";
                };
                
                public partial class SomeOtherClass
                {
                    public float Prop = 0;
                    public int P2 {get; set;} = 4;
                };
                """);

            var runResult = Compile(inputCompilation, out var outputCompilation, out var diagnostics);

            Debug.Assert(runResult.GeneratedTrees.Length == 1);
            Debug.Assert(runResult.Diagnostics.IsEmpty);
            Debug.Assert(runResult.Results.All(r => r.Exception is null));
        }

    }
}