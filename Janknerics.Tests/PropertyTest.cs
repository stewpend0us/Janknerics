using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Janknerics.Tests;

[TestClass]
public class PropertyTest
{
    [TestMethod]
    public void Test()
    {
        // Create the 'input' compilation that the generator will act on
        var inputCompilation = Utils.CreateCompilation("PropertyTestClasses.cs");

        var runResult = Utils.Compile(inputCompilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(1, runResult.GeneratedTrees.Length);
        Assert.IsTrue(runResult.Diagnostics.IsEmpty);
        Assert.IsTrue(runResult.Results.All(r => r.Exception is null));
    }

}