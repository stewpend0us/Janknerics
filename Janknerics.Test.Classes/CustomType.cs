namespace Janknerics.Test.Classes;

public interface IExampleInterface;
public partial class CustomTypeGenerated;

// the class to be used as a template by Janknerics
public class CustomTypeTemplate
{
    [Jankneric(typeof(CustomTypeGenerated), ReplacementType = typeof(IExampleInterface))]
    public float F;
};

public class CustomTypeExpected
{
    public IExampleInterface F;
};

