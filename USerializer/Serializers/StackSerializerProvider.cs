using System;
using System.Collections.Generic;

namespace USerialization
{
    public class StackSerializerProvider : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer dataSerializer)
        {
            dataSerializer = default;

            if (!type.IsGenericType)
                return false;

            if (type.GetGenericTypeDefinition() != typeof(Stack<>))
                return false;
            var constructionArguments = type.GetGenericArguments();
            var elementType = constructionArguments[0];

            if (serializer.TryGetDataSerializer(elementType, out var elementDataSerializer, false) == false)
            {
                dataSerializer = default;
                return false;
            }

            dataSerializer = new StackSerializer(type, elementType, elementDataSerializer);
            return true;
        }
    }
}
