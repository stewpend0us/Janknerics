using Janknerics.Attributes;

namespace Janknerics.Test.Classes;

public partial class PrimaryConstructorGenerated;

[JanknericConstructor(typeof(PrimaryConstructorGenerated))]
public class PrimaryConstructorTemplate(float input)
{
    [Jankneric(typeof(PrimaryConstructorGenerated), NewType = typeof(int))]
    public float F = input;
};

class PrimaryConstructorExpected
{
    public PrimaryConstructorExpected(PrimaryConstructorTemplate source)
    {
        F = (int)source.F;
    }
    public int F;
};
