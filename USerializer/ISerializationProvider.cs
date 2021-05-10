using System;

namespace USerialization
{
    public interface ISerializationProvider
    {
        bool TryGet(USerializer serializer, Type type, out DataSerializer dataSerializer);
    }
}