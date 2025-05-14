using Janknerics.Attributes;

namespace Janknerics.Test.Classes;

public partial class MultipleDifferentGenerated1;

public partial class MultipleDifferentGenerated2;
 

public class MultipleDifferentTemplate
{
    [Jankneric(typeof(MultipleDifferentGenerated1), NewType = typeof(double))]
    public float F = 0;

    [Jankneric(typeof(MultipleDifferentGenerated2), NewType = typeof(int))]
    public long P { get; set; } = 0;
};

public class MultipleDifferentExpected1
{
    public double F = 0;
};

public class MultipleDifferentExpected2
{
    public int P { get; set; } = 0;
};

