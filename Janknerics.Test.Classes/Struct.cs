namespace Janknerics.Test.Classes;
public partial struct StructGenerated;

public struct StructTemplate
{
    [Jankneric(typeof(StructGenerated),NewType = typeof(double))]
    public float F;
};

struct StructExpected
{
    public double F;
};
