namespace Janknerics.Generated.Tests;
using Janknerics;

// the class to be written by Janknerics
public partial class GeneratedClass;

// the class to be used as a template by Janknerics
[Jankneric(typeof(GeneratedClass))]
class TemplateClass
{
    [Jankneric(typeof(GeneratedClass), typeof(double))]
    public float P1 = 0;
};

// the expected output of Janknerics
class ExpectedClass
{
    public double P1 = 0;
};
