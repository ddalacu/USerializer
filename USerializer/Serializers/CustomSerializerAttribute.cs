using System;

namespace USerialization
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class CustomSerializerAttribute : Attribute
    {
        private readonly Type _serializerType;

        public Type SerializerType => _serializerType;

        public CustomSerializerAttribute(Type serializerType)
        {
            _serializerType = serializerType;
        }
    }
}