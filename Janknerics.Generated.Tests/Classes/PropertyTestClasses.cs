namespace Janknerics.Generated.Tests.Classes;

public partial class PropertyTestGenerated;


[Jankneric(typeof(PropertyTestGenerated))]
class PropertyTestTemplate
{
    [Jankneric(typeof(PropertyTestGenerated), typeof(int))]
    public float P { get; set; }  = 0;
}

class PropertyTestExpected
{
    public int P { get; set; }  = 0;
};

