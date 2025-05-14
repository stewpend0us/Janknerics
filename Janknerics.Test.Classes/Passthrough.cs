using Janknerics.Attributes;

namespace Janknerics.Test.Classes;

public partial class PassthroughGenerated;

public class PassthroughTemplate
{
    [Jankneric(typeof(PassthroughGenerated))]
    public string P { get; set; }  = "";
    
    [Jankneric(typeof(PassthroughGenerated))]
    public int F = 0;
}

public class PassthroughExpected
{
    public string P { get; set; }  = "";
    public int F = 0;
};
