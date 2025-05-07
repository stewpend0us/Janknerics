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
    [DataRow(typeof(MultipleTestGenerated1), typeof(MultipleTestExpected1))]
    [DataRow(typeof(MultipleTestGenerated2), typeof(MultipleTestExpected2))]
    [DataRow(typeof(CustomTypeTestGenerated), typeof(CustomTypeTestExpected))]
    public void Test(Type generated, Type expected)
    {
        var gps = generated.GetMembers();
        var eps = expected.GetMembers();
        Assert.AreEqual(eps.Length, gps.Length);
        for (int i = 0; i < eps.Length; i++)
        {
            Assert.AreEqual(eps[i].Name, gps[i].Name);
        }
        //var ga= generated.GetGenericArguments();
        //var ea= expected.GetGenericArguments();
        //var gc= generated.GetGenericParameterConstraints();
        //var ec= expected.GetGenericParameterConstraints();
    }
}