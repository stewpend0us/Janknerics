namespace Janknerics.Test.Classes;

public interface IExampleInterface;
public partial class CustomTypeTestGenerated;

// the class to be used as a template by Janknerics
[Jankneric(typeof(CustomTypeTestGenerated))]
public class CustomTypeTestTemplate
{
    [Jankneric(typeof(CustomTypeTestGenerated), typeof(IExampleInterface))]
    public float F;
};

public class CustomTypeTestExpected
{
    public IExampleInterface F;
};

