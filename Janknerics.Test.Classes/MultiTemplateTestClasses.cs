namespace Janknerics.Test.Classes;

public partial class MultiTemplateTestGenerated1;
public partial class MultiTemplateTestGenerated2;

[Jankneric(typeof(MultiTemplateTestGenerated1))]
public class MultiTemplateTestTemplate1
{
    [Jankneric(typeof(MultiTemplateTestGenerated1), typeof(double))]
    public float F = 0;
};

[Jankneric(typeof(MultiTemplateTestGenerated2))]
public class MultiTemplateTestTemplate2
{
    [Jankneric(typeof(MultiTemplateTestGenerated2), typeof(long))]
    public int F = 0;
};

public class MultiTemplateTestExpected1
{
    public double F = 0;
};

public class MultiTemplateTestExpected2
{
    public long F = 0;
};

