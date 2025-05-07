namespace Janknerics.Generated.Tests.Classes;
public partial class CustomTypeTestGenerated;

// the class to be used as a template by Janknerics
[Jankneric(typeof(CustomTypeTestGenerated))]
class CustomTypeTestTemplate
{
    [Jankneric(typeof(CustomTypeTestGenerated), typeof(IExampleInterface))]
    public float F;
};

class CustomTypeTestExpected
{
    public IExampleInterface F;
};

