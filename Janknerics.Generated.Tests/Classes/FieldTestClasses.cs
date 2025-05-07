namespace Janknerics.Generated.Tests.Classes;
public partial class FieldTestGenerated;

// the class to be used as a template by Janknerics
[Jankneric(typeof(FieldTestGenerated))]
class FieldTestTemplate
{
    [Jankneric(typeof(FieldTestGenerated), typeof(double))]
    public float F = 0;
};

class FieldTestExpected
{
    public double F = 0;
};

