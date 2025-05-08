using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Janknerics.Tests;

[TestClass]
public class BasicTests
{
    [DataTestMethod]
    [DataRow(null, 0, 0)]
    [DataRow("SingleField.cs", 1, 0)]
    [DataRow("SingleProperty.cs", 1, 0)]
    [DataRow("PassthroughField.cs", 1, 0)]
    [DataRow("MultipleTemplates.cs", 2, 0)]
    [DataRow("CustomType.cs", 1, 0)]
    [DataRow("GenerateMultiple.cs", 2, 0)]
    //[DataRow("MissingClassAttribute.cs", 1, 0)]
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