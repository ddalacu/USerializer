using System;

namespace USerialization
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class LocalModuleInitializeAttribute : Attribute
    {
        public Type TargetType { get; }
        public string Method { get; }
        public int Order { get; }

        public LocalModuleInitializeAttribute(Type targetType, string method, int order = 0)
        {
            TargetType = targetType;
            Method = method;
            Order = order;
        }
    }
}