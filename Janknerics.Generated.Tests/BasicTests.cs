using Janknerics.Generated.Tests.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Janknerics.Generated.Tests;

[TestClass]
public class BasicTests
{
    private static void RunTest<TE>(Func<object> getter, TE expectedValue)
    {
        var value = getter();
        Assert.IsInstanceOfType<TE>(value);
        Assert.AreEqual(expectedValue, value);
    }
    
    [TestMethod]
    public void TestField()
    {
        FieldTestTemplate t = new();
        RunTest(()=>t.F, 0.0f);
        
        FieldTestGenerated g = new ();
        RunTest(()=>g.F, 0.0);
    }
    
    [TestMethod]
    public void TestProperty()
    {
        PropertyTestTemplate t = new ();
        RunTest(()=>t.P, 0.0f);
        
        PropertyTestGenerated g = new ();
        RunTest(()=>g.P, 0);
    }
    
    [TestMethod]
    public void TestPassthrough()
    {
        PassthroughTestTemplate t = new ();
        RunTest(()=>t.P, "");
        
        PassthroughTestGenerated g = new ();
        RunTest(()=>g.P, "");
    }
    
    [TestMethod]
    public void TestMultiple()
    {
        MultipleTestTemplate t = new ();
        RunTest(()=>t.F, 0.0f);
        
        MultipleTestGenerated1 g1 = new ();
        MultipleTestGenerated2 g2 = new ();
        RunTest(()=>g1.F, 0.0);
        RunTest(()=>g2.F, 0);
    }

    [TestMethod]
    public void TestCustomType()
    {
        CustomTypeTestTemplate t = new ();
        Assert.IsInstanceOfType<float>(t.F);
        
        CustomTypeTestGenerated g = new ();
        Assert.AreEqual(typeof(IExampleInterface), typeof(CustomTypeTestGenerated).GetField("F")?.FieldType);
    }
}
