using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Janknerics.Generated.Tests;

// the class to be written by Janknerics
public partial class GeneratedClass;

// the class to be used as a template by Janknerics
[Jankneric(typeof(GeneratedClass))]
class TemplateClass
{
    [Jankneric(typeof(GeneratedClass), typeof(double))]
    public float P1 = 0;

    [Jankneric(typeof(GeneratedClass), typeof(int))]
    public float P2 { get; set; }  = 4;
    
    [Jankneric(typeof(GeneratedClass))]
    public string P3 { get; set; }  = "4";
};

// the expected output of Janknerics
class ExpectedClass
{
    public double P1 = 0;
    public int P2 { get; set; }  = 4;
};

[TestClass]
public class Test1
{
    [TestMethod]
    public void Test()
    {
        GeneratedClass gps = new();
        Debug.Assert(gps.P1 is double);
        Debug.Assert(gps.P1 == 0);
        Debug.Assert(gps.P2 is int);
        Debug.Assert(gps.P2 == 4);
        Debug.Assert(gps.P3 is string);
        Debug.Assert(gps.P3 == "4");
    }
}
