using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Janknerics.Tests
{
    public partial class JanknericsTests
    {
        [TestMethod]
        public void NothingTest()
        {
            // Create the 'input' compilation that the generator will act on
            var inputCompilation = CreateCompilation();

            // Run the generation pass
            // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
            var runResult = Compile(inputCompilation, out var outputCompilation, out var diagnostics);

            // The runResult contains the combined results of all generators passed to the driver
            Debug.Assert(runResult.GeneratedTrees.Length == 0);
            Debug.Assert(runResult.Diagnostics.IsEmpty);
            Debug.Assert(runResult.Results.All(r => r.Exception is null));
        }

    }
}