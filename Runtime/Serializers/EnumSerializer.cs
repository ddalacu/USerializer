using System;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public class EnumSerializer : ISerializationProvider
    {
        private USerializer _serializer;

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
        }

        public void Start(USerializer serializer)
        {
            
        }

        public bool TryGetSerializationMethods(Type type, out DataSerializer serializationMethods)
        {
            if (type.IsArray)
            {
                type = type.GetElementType();
                if (type.IsEnum == false)
                {
                    serializationMethods = default;
                    return false;
                }

                var primitiveType = type.GetEnumUnderlyingType();
                var makeArrayType = primitiveType.MakeArrayType();
                return _serializer.TryGetSerializationMethods(makeArrayType, out serializationMethods);
            }

            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];

                if (type.IsEnum == false)
                {
                    serializationMethods = default;
                    return false;
                }

                var primitiveType = type.GetEnumUnderlyingType();
                var makeListType = typeof(List<>).MakeGenericType(primitiveType);

                return _serializer.TryGetSerializationMethods(makeListType, out serializationMethods);
            }

            if (type.IsEnum == false)
            {
                serializationMethods = default;
                return false;
            }

            var enumType = type.GetEnumUnderlyingType();

            return _serializer.TryGetSerializationMethods(enumType, out serializationMethods);
        }
    }

}