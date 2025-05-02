using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Janknerics.Generated.Tests;

[TestClass]
public class Tester
{
    [DataTestMethod]
    [DataRow(typeof(GeneratedClass1), typeof(ExpectedClass1))]
    public void Test(Type generated, Type expected)
    {
        var gp = generated.GetMembers();
        var ep = expected.GetMembers();
        //var ga= generated.GetGenericArguments();
        //var ea= expected.GetGenericArguments();
        //var gc= generated.GetGenericParameterConstraints();
        //var ec= expected.GetGenericParameterConstraints();
    }
}