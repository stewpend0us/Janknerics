using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Janknerics.Tests;

[TestClass]
public class BasicTests
{
    [DataTestMethod]
    [DataRow(null, 0, 0)]
    [DataRow("FieldTestClasses.cs", 1, 0)]
    [DataRow("PropertyTestClasses.cs", 1, 0)]
    [DataRow("PassthroughTestClasses.cs", 1, 0)]
    [DataRow("MultipleTestClasses.cs", 2, 0)]
    [DataRow("CustomTypeTestClasses.cs", 1, 0)]
    [DataRow("MultiTemplateTestClasses.cs", 2, 0)]
    public void Test(string? filename, int expectedTrees, int expectedDiagnostics)
    {
        // Create the 'input' compilation that the generator will act on
        var inputCompilation = Utils.CreateCompilation(filename);

        var runResult = Utils.Compile(inputCompilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(expectedTrees, runResult.GeneratedTrees.Length);
        Assert.AreEqual(expectedDiagnostics, runResult.Diagnostics.Length);
        Assert.IsTrue(runResult.Results.All(r => r.Exception is null));
    }

}