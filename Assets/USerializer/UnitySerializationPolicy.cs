using System;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

namespace USerialization
{
    public class UnitySerializationPolicy : ISerializationPolicy
    {
        public bool ShouldSerialize(Type type)
        {
            if (type.IsAbstract)
                return false;

            if (type.IsGenericType)// Type<int>
                return false;

            if (type.IsGenericTypeDefinition)// Type<>
                return false;

            if (type.IsClass)
            {
                if (typeof(Object).IsAssignableFrom(type))
                    return true;

                if (type.GetCustomAttribute<SerializableAttribute>() != null)
                    return true;
            }

            if (type.IsValueType)
            {
                return true;
            }

            return false;
        }

        public bool ShouldSerialize(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPrivate)
            {
                if (Attribute.IsDefined(fieldInfo, typeof(SerializeField)) == false)//todo cache typeof
                    return false;
            }
            else
            {
                if (Attribute.IsDefined(fieldInfo, typeof(NonSerializedAttribute))) //todo cache typeof
                    return false;
            }

            return true;
        }
    }
}