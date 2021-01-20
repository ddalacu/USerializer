using System;
using System.Reflection;

namespace USerialization
{
    public interface ISerializationPolicy
    {
        bool ShouldSerialize(Type type);

        bool ShouldSerialize(FieldInfo fieldInfo);
    }
}