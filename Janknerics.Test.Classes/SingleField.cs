namespace Janknerics.Test.Classes;
public partial class FieldGenerated;

// the class to be used as a template by Janknerics
[Jankneric(typeof(FieldGenerated))]
public class FieldTemplate
{
    [Jankneric(typeof(FieldGenerated), typeof(double))]
    public float F = 0;
};

class FieldExpected
{
    public double F = 0;
};

