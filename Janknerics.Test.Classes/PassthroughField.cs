namespace Janknerics.Test.Classes;

public partial class PassthroughGenerated;

public class PassthroughTemplate
{
    [Jankneric(typeof(PassthroughGenerated))]
    public string P { get; set; }  = "";
}

public class PassthroughExpected
{
    public string P { get; set; }  = "";
};
