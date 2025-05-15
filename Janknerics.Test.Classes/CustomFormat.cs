using Janknerics.Attributes;

namespace Janknerics.Test.Classes;
public partial class CustomFormatGenerated;

[JanknericConstructor(typeof(CustomFormatGenerated))]
public class CustomFormatTemplate
{
    [Jankneric(typeof(CustomFormatGenerated), NewType = typeof(double), Template = "{LeftName} = ({LeftType})({RightType}){RightName};")]
    public float F1 = 0;
    
    [Jankneric(typeof(CustomFormatGenerated), NewType = typeof(double), Template = $"{{LeftName}} = ({{LeftType}})({{RightType}}){{RightName}};")]
    public float F2 = 0;
};

class CustomFormatExpected
{
    public double F1 = 0;
    public double F2 = 0;
};
