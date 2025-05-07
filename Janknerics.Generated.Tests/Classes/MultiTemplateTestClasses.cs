namespace Janknerics.Generated.Tests.Classes;

public partial class MultiTemplateTestGenerated1;
public partial class MultiTemplateTestGenerated2;

[Jankneric(typeof(MultiTemplateTestGenerated1))]
class MultiTemplateTestTemplate1
{
    [Jankneric(typeof(MultiTemplateTestGenerated1), typeof(double))]
    public float F = 0;
};

[Jankneric(typeof(MultiTemplateTestGenerated2))]
class MultiTemplateTestTemplate2
{
    [Jankneric(typeof(MultiTemplateTestGenerated2), typeof(long))]
    public int F = 0;
};

class MultiTemplateTestExpected1
{
    public double F = 0;
};

class MultiTemplateTestExpected2
{
    public double F = 0;
};

