namespace Janknerics.Generated.Tests.Classes;

public partial class PassthroughTestGenerated;

[Jankneric(typeof(PassthroughTestGenerated))]
class PassthroughTestTemplate
{
    [Jankneric(typeof(PassthroughTestGenerated))]
    public string P { get; set; }  = "";
}

class PassthroughTestExpected
{
    public string P { get; set; }  = "";
};
