namespace Janknerics.Test.Classes;

public partial class PropertyTestGenerated;


[Jankneric(typeof(PropertyTestGenerated))]
public class PropertyTestTemplate
{
    [Jankneric(typeof(PropertyTestGenerated), typeof(int))]
    public float P { get; set; }  = 0;
}

public class PropertyTestExpected
{
    public int P { get; set; }  = 0;
};

