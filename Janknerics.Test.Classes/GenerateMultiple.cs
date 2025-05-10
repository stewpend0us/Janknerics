namespace Janknerics.Test.Classes;

public partial class MultipleGenerated1;
public partial class MultipleGenerated2;

// the class to be used as a template by Janknerics
public class MultipleTemplate
{
    [Jankneric(typeof(MultipleGenerated1), ReplacementType = typeof(double))]
    [Jankneric(typeof(MultipleGenerated2), ReplacementType = typeof(int))]
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

