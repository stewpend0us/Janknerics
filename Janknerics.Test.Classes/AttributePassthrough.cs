namespace Janknerics.Test.Classes;

public class JankTestAttribute : Attribute;

public partial class AttributeGenerated;

[JankTest]
public class AttributeTemplate
{
    [JankTest]
    [Jankneric(typeof(AttributeGenerated),NewType = typeof(double))]
    public float F = 0;
};

[JankTest]
class AttributeExpected
{
    [JankTest]
    public double F = 0;
};

