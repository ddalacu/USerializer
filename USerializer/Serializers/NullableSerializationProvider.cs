using System;

namespace USerialization
{
    public class NullableSerializationProvider : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer dataSerializer)
        {
            dataSerializer = default;

            //check if is nullable
            
            if (type.IsGenericType == false)
                return false;
            if (type.GetGenericTypeDefinition() != typeof(Nullable<>))
                return false;
            
            dataSerializer = new StructDataSerializer(type, (fieldInfo) => true);

            return true;
        }
    }
}