using Janknerics.Generated.Tests.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Janknerics.Generated.Tests;

[TestClass]
public class BasicTests
{
    [TestMethod]
    public void TestField()
    {
        FieldTestTemplate t = new();
        Assert.IsTrue(t.F is float);
        Assert.AreEqual(t.F, 0);
        
        FieldTestGenerated g = new ();
        Assert.IsTrue(g.F is double);
        Assert.AreEqual(g.F, 0);
    }
    
    [TestMethod]
    public void TestProperty()
    {
        PropertyTestTemplate t = new ();
        Assert.IsTrue(t.P is float);
        Assert.AreEqual(t.P, 0);
        
        PropertyTestGenerated g = new ();
        Assert.IsTrue(g.P is int);
        Assert.AreEqual(g.P, 0);
    }
    
    [TestMethod]
    public void TestPassthrough()
    {
        PassthroughTestTemplate t = new ();
        Assert.IsTrue(t.P is string);
        Assert.AreEqual(t.P, "");
        
        PassthroughTestGenerated g = new ();
        Assert.IsTrue(g.P is string);
        Assert.AreEqual(g.P, "");
    }
}
