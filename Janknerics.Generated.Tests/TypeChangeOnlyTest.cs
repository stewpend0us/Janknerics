using System.Reflection;
using Janknerics.Test.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Janknerics.Generated.Tests;

[TestClass]
public class TypeChangeOnlyTest
{
    [DataTestMethod]
    [DataRow(typeof(FieldTestGenerated), typeof(FieldTestGenerated))]
    [DataRow(typeof(PropertyTestGenerated), typeof(PropertyTestExpected))]
    [DataRow(typeof(PassthroughTestGenerated), typeof(PassthroughTestExpected))]
    [DataRow(typeof(MultipleTestGenerated1), typeof(MultipleTestExpected1))]
    [DataRow(typeof(MultipleTestGenerated2), typeof(MultipleTestExpected2))]
    [DataRow(typeof(CustomTypeTestGenerated), typeof(CustomTypeTestExpected))]
    [DataRow(typeof(MultiTemplateTestGenerated1), typeof(MultiTemplateTestExpected1))]
    [DataRow(typeof(MultiTemplateTestGenerated2), typeof(MultiTemplateTestExpected2))]
    public void Test(Type generated, Type expected)
    {
        var gps = generated.GetMembers();
        var eps = expected.GetMembers();
        Assert.AreEqual(eps.Length, gps.Length);
        for (var i = 0; i < eps.Length; i++)
        {
            Assert.AreEqual(eps[i].GetType(), gps[i].GetType());
            Assert.AreEqual(eps[i].Name, gps[i].Name);
            CollectionAssert.AreEqual(eps[i].GetCustomAttributes().ToArray(), gps[i].GetCustomAttributes().ToArray());
            switch (eps[i])
            {
                case FieldInfo e:
                {
                    var g = (FieldInfo)gps[i];
                    Assert.AreEqual(e.FieldType, g.FieldType);
                    break;
                }
                case PropertyInfo e:
                {
                    var g = (PropertyInfo)gps[i];
                    Assert.AreEqual(e.PropertyType, g.PropertyType);
                    break;
                }
                case MethodBase e:
                {
                    var g = (MethodBase)gps[i];
                    if (e is MethodInfo em && g is MethodInfo gm)
                        Assert.AreEqual(em.ReturnType, gm.ReturnType);
                    var ep = e.GetParameters();
                    var gp = g.GetParameters();
                    Assert.AreEqual(ep.Length, gp.Length);
                    for (var j = 0; j < ep.Length; j++)
                    {
                        Assert.AreEqual(ep[j].Name, gp[j].Name);
                        Assert.AreEqual(ep[j].ParameterType, gp[j].ParameterType);
                        Assert.AreEqual(ep[j].Attributes, gp[j].Attributes);
                    }
                    break;
                }
                default:
                    Assert.Fail("unhandled MemberInfo type in test switch");
                    break;
            }
        }
        //var ga= generated.GetGenericArguments();
        //var ea= expected.GetGenericArguments();
        //var gc= generated.GetGenericParameterConstraints();
        //var ec= expected.GetGenericParameterConstraints();
    }
}