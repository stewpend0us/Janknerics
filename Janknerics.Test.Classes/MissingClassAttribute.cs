namespace Janknerics.Test.Classes;
public partial class MissingClassAttributeGenerated;

public class MissingClassAttributeTestTemplate
{
    [Jankneric(typeof(MissingClassAttributeGenerated), typeof(double))]
    public float F = 0;
};

class MissingClassAttributeExpected
{
    public double F = 0;
};