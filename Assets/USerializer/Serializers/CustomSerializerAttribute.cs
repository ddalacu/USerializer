using System;
using UnityEngine.Scripting;

namespace USerialization
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class CustomSerializerAttribute : PreserveAttribute
    {
        private readonly Type _serializerType;

        public Type SerializerType => _serializerType;

        public CustomSerializerAttribute(Type serializerType)
        {
            _serializerType = serializerType;
        }
    }
}