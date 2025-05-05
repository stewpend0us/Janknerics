namespace Janknerics.Generated.Tests;
using Janknerics;

// the class to be written by Janknerics
public partial class GeneratedClass1;
public partial class GeneratedClass2;

// the class to be used as a template by Janknerics
[Jankneric(typeof(GeneratedClass1))]
[Jankneric(typeof(GeneratedClass2))]
class TemplateClass1
{
    [Jankneric(typeof(GeneratedClass1), typeof(double))]
    public float P1 = 0;
    
    [Jankneric(typeof(GeneratedClass2), typeof(double))]
    public float P2 = 0;
};

// the expected output of Janknerics
class ExpectedClass1
{
    public double P1 = 0;
};

class ExpectedClass2
{
    public double P2 = 0;
};
