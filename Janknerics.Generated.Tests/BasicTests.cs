using Janknerics.Test.Classes;
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
        FieldTemplate t = new();
        RunTest(()=>t.F, 0.0f);
        
        FieldGenerated g = new ();
        RunTest(()=>g.F, 0.0);
    }
    
    [TestMethod]
    public void TestProperty()
    {
        PropertyTemplate t = new ();
        RunTest(()=>t.P, 0.0f);
        
        PropertyGenerated g = new ();
        RunTest(()=>g.P, 0);
    }
    
    [TestMethod]
    public void TestPassthrough()
    {
        PassthroughTemplate t = new ();
        RunTest(()=>t.P, "");
        RunTest(()=>t.F, 0);
        
        PassthroughGenerated g = new ();
        RunTest(()=>g.P, "");
        RunTest(()=>g.F, 0);
    }
    
    [TestMethod]
    public void TestMultiple()
    {
        MultipleTemplate t = new ();
        RunTest(()=>t.F, 0.0f);
        
        MultipleGenerated1 g1 = new ();
        MultipleGenerated2 g2 = new ();
        RunTest(()=>g1.F, 0.0);
        RunTest(()=>g2.F, 0);
    }

    [TestMethod]
    public void TestMultipleDifferent()
    {
        MultipleDifferentTemplate t = new ();
        RunTest(()=>t.F, 0.0f);
        RunTest(()=>t.P, 0L);
        
        MultipleDifferentGenerated1 g1 = new ();
        MultipleDifferentGenerated2 g2 = new ();
        RunTest(()=>g1.F, 0.0);
        RunTest(()=>g2.P, 0);
    }
    
    [TestMethod]
    public void TestCustomType()
    {
        CustomTypeTemplate t = new ();
        Assert.IsInstanceOfType<float>(t.F);
        
        CustomTypeGenerated g = new ();
        Assert.AreEqual(typeof(IExampleInterface), typeof(CustomTypeGenerated).GetField("F")?.FieldType);
    }

    [TestMethod]
    public void TestMultiTemplate()
    {
        MultiTemplateTemplate1 t1 = new ();
        MultiTemplateTemplate2 t2 = new ();
        RunTest(()=>t1.F, 0.0f);
        RunTest(()=>t2.F, 0);
        
        MultiTemplateGenerated1 g1 = new ();
        MultiTemplateGenerated2 g2 = new ();
        RunTest(()=>g1.F, 0.0);
        RunTest(()=>g2.F, 0L);
    }
    
    [TestMethod]
    public void TestDifferentNamespace()
    {
        Janknerics.Test.DifferentClasses.FieldTemplate t = new();
        RunTest(()=>t.F, 0.0f);
        
        Janknerics.Test.DifferentClasses.FieldGenerated g = new ();
        RunTest(()=>g.F, 0.0);
    }
    
    [TestMethod]
    public void AttributePassthrough()
    {
        AttributeTemplate t = new();
        RunTest(()=>t.F, 0.0f);
        
        AttributeGenerated g = new ();
        RunTest(()=>g.F, 0.0);
    }
    
    [TestMethod]
    public void Struct()
    {
        StructTemplate t = new();
        RunTest(()=>t.F, 0.0f);
        
        StructGenerated g = new ();
        RunTest(()=>g.F, 0.0);
    }
    
    [TestMethod]
    public void Record()
    {
        RecordTemplate t = new();
        RunTest(()=>t.F, 0.0f);
        
        RecordGenerated g = new ();
        RunTest(()=>g.F, 0.0);
    }
    
    [TestMethod]
    public void Constructor()
    {
        ConstructorTemplate t = new();
        RunTest(()=>t.P, 0.0f);
        RunTest(()=>t.F, 0.0f);
        
        // change before passing to make sure the value flows through
        t.P = 1;
        t.F = 2;
        ConstructorGenerated g = new (t);
        RunTest(()=>g.P, 1);
        RunTest(()=>g.F, 2);
    }
}
