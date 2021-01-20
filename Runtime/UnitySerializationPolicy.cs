using System;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

namespace USerialization
{
    public class UnitySerializationPolicy : ISerializationPolicy
    {
        private readonly Type _serializeFieldAttributeType = typeof(SerializeField);

        private readonly Type _nonSerializedAttributeType = typeof(SerializeField);

        private readonly Type _unityObjectType = typeof(UnityEngine.Object);

        private readonly Type _serializableAttributeType = typeof(SerializableAttribute);


        public bool ShouldSerialize(Type type)
        {
            if (type.IsAbstract)
                return false;

            if (type.IsGenericType)// Type<int>
                return false;

            if (type.IsGenericTypeDefinition)// Type<>
                return false;

            if (type.IsValueType)
                return true;

            if (type.IsClass)
            {
                if (_unityObjectType.IsAssignableFrom(type))
                    return true;

                if (type.GetCustomAttribute(_serializableAttributeType) != null)
                    return true;
            }

            return false;
        }

        public bool ShouldSerialize(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPrivate)
            {
                if (Attribute.IsDefined(fieldInfo, _serializeFieldAttributeType) == false)
                    return false;
            }
            else
            {
                if (Attribute.IsDefined(fieldInfo, _nonSerializedAttributeType))
                    return false;
            }

            return true;
        }
    }
}