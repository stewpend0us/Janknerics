using System.Diagnostics;

namespace Janknerics.Generated.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// the class to be written by Janknerics
public partial class GeneratedClass3;

// the class to be used as a template by Janknerics
[Jankneric(typeof(GeneratedClass3))]
class TemplateClass3
{
    [Jankneric(typeof(GeneratedClass3), typeof(ITestMethod))]
    public float P1;
};

// the expected output of Janknerics
class ExpectedClass3
{
    public ITestMethod P1;
};
