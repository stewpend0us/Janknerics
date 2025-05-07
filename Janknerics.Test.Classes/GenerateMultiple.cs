namespace Janknerics.Test.Classes;

public partial class MultipleGenerated1;
public partial class MultipleGenerated2;

// the class to be used as a template by Janknerics
[Jankneric(typeof(MultipleGenerated1))]
[Jankneric(typeof(MultipleGenerated2))]
public class MultipleTemplate
{
    [Jankneric(typeof(MultipleGenerated1), typeof(double))]
    [Jankneric(typeof(MultipleGenerated2), typeof(int))]
    public float F = 0;
};

public class MultipleExpected1
{
    public double F = 0;
};

public class MultipleExpected2
{
    public int F = 0;
};

