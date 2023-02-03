using System;
using System.Collections.Generic;
using System.Reflection;
using USerialization;

namespace USerializerTests
{
    public class SerializeFieldAttribute : Attribute
    {

    }

    public class FormerlySerializedAsAttribute : Attribute
    {
        public string OldName { get; }

        public FormerlySerializedAsAttribute(string oldName)
        {
            OldName = oldName;
        }
    }

    public class UnitySerializationPolicy : ISerializationPolicy
    {
        private readonly Type _serializeFieldAttributeType = typeof(SerializeFieldAttribute);

        private readonly Type _nonSerializedAttributeType = typeof(NonSerializedAttribute);

        private readonly Type _serializableAttributeType = typeof(SerializableAttribute);

        public Func<FieldInfo, bool> ShouldSerializeField;

        public bool ShouldSerialize(Type type)
        {
            if (type.IsAbstract)
                return false;

            if (type.IsGenericTypeDefinition)// Type<>
                return false;

            if (type.IsValueType)
                return true;

            if (type.IsClass)
            {
                if (Attribute.IsDefined(type, _serializableAttributeType))
                {
                    if (type.IsGenericType)
                    {
                        if (ShouldTryToSerializeGeneric(type) == false)
                            return false;
                    }

                    return true;
                }
            }

            return false;
        }

        private bool ShouldTryToSerializeGeneric(Type type)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(Dictionary<,>))
                return false;
            if (genericTypeDefinition == typeof(HashSet<>))
                return false;
            if (genericTypeDefinition == typeof(Stack<>))
                return false;
            if (genericTypeDefinition == typeof(Queue<>))
                return false;
            if (genericTypeDefinition == typeof(List<>))
                return false;

            return true;
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

            if (ShouldSerializeField != null &&
                ShouldSerializeField(fieldInfo) == false)
                return false;

            return true;
        }

        public string[] GetAlternateNames(FieldInfo fieldInfo)
        {
            var formerly = (FormerlySerializedAsAttribute[])Attribute.GetCustomAttributes(fieldInfo, typeof(FormerlySerializedAsAttribute));

            var length = formerly.Length;

            if (length == 0)
                return null;

            var old = new string[length];

            for (var index = 0; index < length; index++)
            {
                old[index] = formerly[index].OldName;
            }

            return old;
        }
    }


}
