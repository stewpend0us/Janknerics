﻿using Janknerics.Attributes;

namespace Janknerics.Test.Classes;
public partial record RecordGenerated;

public record RecordTemplate
{
    [Jankneric(typeof(RecordGenerated),NewType = typeof(double))]
    public float F = 0;
};

record RecordExpected
{
    public double F = 0;
};
