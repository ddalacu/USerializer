using System;

namespace USerialization
{
    public class EnumSerializer : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer serializationMethods)
        {
            if (type.IsEnum)
            {
                var enumType = type.GetEnumUnderlyingType();
                return serializer.TryGetDataSerializer(enumType, out serializationMethods);
            }

            serializationMethods = default;
            return false;
        }
    }

}