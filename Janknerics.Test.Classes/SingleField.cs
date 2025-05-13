namespace Janknerics.Test.Classes;
public partial class FieldGenerated;

public class FieldTemplate
{
    [Jankneric(typeof(FieldGenerated),NewType = typeof(double))]
    public float F = 0;
};

class FieldExpected
{
    public double F = 0;
};
