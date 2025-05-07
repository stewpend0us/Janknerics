namespace Janknerics.Generated.Tests.Classes;

public partial class MultipleTestGenerated1;
public partial class MultipleTestGenerated2;

// the class to be used as a template by Janknerics
[Jankneric(typeof(MultipleTestGenerated1))]
[Jankneric(typeof(MultipleTestGenerated2))]
class MultipleTestTemplate
{
    [Jankneric(typeof(MultipleTestGenerated1), typeof(double))]
    [Jankneric(typeof(MultipleTestGenerated2), typeof(int))]
    public float F = 0;
};

class MultipleTestExpected1
{
    public double F = 0;
};

class MultipleTestExpected2
{
    public double F = 0;
};

