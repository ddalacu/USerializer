using System;
using System.Collections.Generic;

namespace USerialization
{
    public class DictionarySerializerProvider : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer dataSerializer)
        {
            dataSerializer = default;

            if (!type.IsGenericType)
                return false;
            
            if (type.GetGenericTypeDefinition() != typeof(Dictionary<,>)) 
                return false;
            
            var constructionArguments = type.GetGenericArguments();
            var genericType = typeof(DictionarySerializer<,>).MakeGenericType(constructionArguments[0], constructionArguments[1]);
            var instance= Activator.CreateInstance(genericType);
            dataSerializer = (DataSerializer)instance;
            return true;
        }
    }
}