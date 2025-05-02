namespace Janknerics.Generated.Tests;
using Janknerics;

// the class to be written by Janknerics
partial class GeneratedClass1;

// the class to be used as a template by Janknerics
[Jankneric(typeof(GeneratedClass1))]
class TemplateClass1
{
    [Jankneric(typeof(GeneratedClass1), typeof(double))]
    public float P1 = 0;
}

// the expected output of Janknerics
partial class ExpectedClass1
{
    public double P1 = 0;
}
