namespace Janknerics.Test.Classes;

public partial class ConstructorGenerated;

[JanknericConstructor(typeof(ConstructorGenerated))]
public class ConstructorTemplate
{
    [Jankneric(typeof(ConstructorGenerated), ReplacementType = typeof(int))]
    public float P { get; set; }  = 0;
    [Jankneric(typeof(ConstructorGenerated), ReplacementType = typeof(int))]
    public float F  = 0;
}

public class ConstructorExpected
{
    public ConstructorExpected(ConstructorTemplate template)
    {
        P = (int)template.P;
        F = (int)template.F;
    }
    public int P { get; set; }
    public int F;
};

