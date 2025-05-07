namespace Janknerics.Test.Classes;

public partial class PassthroughTestGenerated;

[Jankneric(typeof(PassthroughTestGenerated))]
public class PassthroughTestTemplate
{
    [Jankneric(typeof(PassthroughTestGenerated))]
    public string P { get; set; }  = "";
}

public class PassthroughTestExpected
{
    public string P { get; set; }  = "";
};
