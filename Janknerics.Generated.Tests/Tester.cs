using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Janknerics.Generated.Tests;

[TestClass]
public class Tester
{
    [DataTestMethod]
    [DataRow(typeof(GeneratedClass), typeof(ExpectedClass))]
    [DataRow(typeof(GeneratedClass1), typeof(ExpectedClass1))]
    [DataRow(typeof(GeneratedClass2), typeof(ExpectedClass2))]
    public void Test(Type generated, Type expected)
    {
        var gps = generated.GetMembers();
        var eps = expected.GetMembers();
        Debug.Assert(gps.Length == eps.Length);
        foreach (var gp in gps.Where(m => m is not MethodInfo or ConstructorInfo))
        {
            Debug.Assert(eps
                .Any(exp=> exp.Name == gp.Name));
        }
        //var ga= generated.GetGenericArguments();
        //var ea= expected.GetGenericArguments();
        //var gc= generated.GetGenericParameterConstraints();
        //var ec= expected.GetGenericParameterConstraints();
    }
}