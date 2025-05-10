namespace Janknerics.Test.Classes;

public partial class PropertyGenerated;

public class PropertyTemplate
{
    [Jankneric(typeof(PropertyGenerated),ReplacementType = typeof(int))]
    public float P { get; set; }  = 0;
}

public class PropertyExpected
{
    public int P { get; set; }  = 0;
};

