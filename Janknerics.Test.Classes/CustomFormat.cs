using Janknerics.Attributes;

namespace Janknerics.Test.Classes;
public partial class CustomFormatGenerated;

[JanknericConstructor(typeof(CustomFormatGenerated))]
public class CustomFormatTemplate
{
    [Jankneric(typeof(CustomFormatGenerated), NewType = typeof(double), Template = "{LeftName} = (LeftType)(RightType){RightName};")]
    public float F = 0;
};

class CustomFormatExpected
{
    public double F = 0;
};
