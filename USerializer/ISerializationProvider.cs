using System;

namespace USerialization
{

    public interface ISerializationProvider
    {
        void Initialize(USerializer serializer);

        void Start(USerializer serializer);

        bool TryGet(USerializer serializer, Type type, out DataSerializer dataSerializer);
    }
}