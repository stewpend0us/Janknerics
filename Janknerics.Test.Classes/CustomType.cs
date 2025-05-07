namespace Janknerics.Test.Classes;

public interface IExampleInterface;
public partial class CustomTypeGenerated;

// the class to be used as a template by Janknerics
[Jankneric(typeof(CustomTypeGenerated))]
public class CustomTypeTemplate
{
    [Jankneric(typeof(CustomTypeGenerated), typeof(IExampleInterface))]
    public float F;
};

public class CustomTypeExpected
{
    public IExampleInterface F;
};

