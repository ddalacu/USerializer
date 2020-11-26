using System;

namespace USerialization
{

    public interface ISerializationProvider
    {
        void Initialize(USerializer serializer);

        void Start(USerializer serializer);

        bool TryGetSerializationMethods(Type type, out SerializationMethods methods);
    }
}