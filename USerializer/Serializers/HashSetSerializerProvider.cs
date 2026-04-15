using System;
using System.Collections.Generic;

namespace USerialization
{
    public class HashSetSerializerProvider : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer dataSerializer)
        {
            dataSerializer = default;

            if (!type.IsGenericType)
                return false;

            if (type.GetGenericTypeDefinition() != typeof(HashSet<>))
                return false;

            var constructionArguments = type.GetGenericArguments();
            var genericType = typeof(HashSetSerializer<>).MakeGenericType(constructionArguments[0]);
            var instance = Activator.CreateInstance(genericType);
            dataSerializer = (DataSerializer)instance;
            return true;
        }
    }
}
