using System;

namespace Janknerics
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class JanknericAttribute : Attribute
    {
        public Type TargetType { get; }
        public Type? ReplacementType { get; }

        public JanknericAttribute(Type targetType)
        {
            TargetType = targetType;
        }

        public JanknericAttribute(Type targetType, Type replacementType)
        {
            TargetType = targetType;
            ReplacementType = replacementType;
        }
    }
}