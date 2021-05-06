using System;
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

        public bool TryGet(Type type, out DataSerializer serializationMethods)
        {
            if (type.IsEnum == false)
            {
                serializationMethods = default;
                return false;
            }

            var enumType = type.GetEnumUnderlyingType();

            return _serializer.TryGetDataSerializer(enumType, out serializationMethods);
        }
    }

}