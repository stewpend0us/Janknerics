using System.Diagnostics;
using System.Reflection;
using Janknerics.Generated.Tests.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Janknerics.Generated.Tests;

[TestClass]
public class Tester
{
    [DataTestMethod]
    [DataRow(typeof(FieldTestGenerated), typeof(FieldTestGenerated))]
    [DataRow(typeof(PropertyTestGenerated), typeof(PropertyTestExpected))]
    [DataRow(typeof(PassthroughTestGenerated), typeof(PassthroughTestExpected))]
    [DataRow(typeof(GeneratedClass3), typeof(ExpectedClass3))]
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