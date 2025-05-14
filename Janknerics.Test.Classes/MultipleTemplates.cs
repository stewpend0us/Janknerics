using Janknerics.Attributes;

namespace Janknerics.Test.Classes;

public partial class MultiTemplateGenerated1;
public partial class MultiTemplateGenerated2;

public class MultiTemplateTemplate1
{
    [Jankneric(typeof(MultiTemplateGenerated1), NewType = typeof(double))]
    public float F = 0;
};

public class MultiTemplateTemplate2
{
    [Jankneric(typeof(MultiTemplateGenerated2),NewType = typeof(long))]
    public int F = 0;
};

public class MultiTemplateExpected1
{
    public double F = 0;
};

public class MultiTemplateExpected2
{
    public long F = 0;
};
