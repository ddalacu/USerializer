using System;
using System.Collections.Generic;

namespace USerialization
{
    public class KeyValuePairSerializationProvider : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer dataSerializer)
        {
            dataSerializer = default;

            if (!type.IsGenericType)
                return false;

            if (type.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
                return false;
            
            dataSerializer = new StructDataSerializer(type, (fieldInfo) =>
            {
                if (string.Equals(fieldInfo.Name, "key", StringComparison.Ordinal) ||
                    string.Equals(fieldInfo.Name, "value", StringComparison.Ordinal))
                    return true;
                
                return false;
            });
            return true;
        }
    }
}