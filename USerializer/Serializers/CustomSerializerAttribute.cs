using System;

namespace USerialization
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class CustomSerializerAttribute : Attribute
    {
        private readonly Type _serializerType;

        private readonly Type _serializedType;

        public Type SerializedType => _serializedType;

        public Type SerializerType => _serializerType;

        public CustomSerializerAttribute(Type serializedType, Type serializerType)
        {
            _serializedType = serializedType;
            _serializerType = serializerType;
        }
    }
}