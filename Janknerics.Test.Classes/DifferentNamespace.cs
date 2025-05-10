namespace Janknerics.Test.DifferentClasses;
public partial class FieldGenerated;

public class FieldTemplate
{
    [Jankneric(typeof(FieldGenerated),ReplacementType = typeof(double))]
    public float F = 0;
};

class FieldExpected
{
    public double F = 0;
};

