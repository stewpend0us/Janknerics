using System.Globalization;

namespace Janknerics.Test.Classes;

public partial class ConstructorGenerated;

[JanknericConstructor(typeof(ConstructorGenerated))]
public class ConstructorTemplate
{
    [Jankneric(typeof(ConstructorGenerated), ReplacementType = typeof(int))]
    public float P { get; set; }  = 0;
    [Jankneric(typeof(ConstructorGenerated), ReplacementType = typeof(int))]
    public float F  = 0;
    
    [Jankneric(typeof(ConstructorGenerated), ReplacementType = typeof(string))]
    public float S  = 0;
}

public class ConstructorExpected
{
    public ConstructorExpected(ConstructorTemplate source)
    {
        P = (int)source.P;
        F = (int)source.F;
        S = source.S.ToString();
    }
    public int P { get; set; }
    public int F;
    public string S;
};

