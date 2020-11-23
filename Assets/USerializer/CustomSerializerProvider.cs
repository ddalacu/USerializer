using System;
using System.Reflection;
using UnityEngine;

namespace USerialization
{
    public unsafe interface ICustomSerializer
    {
        Type SerializedType { get; }

        USerializer Serializer { get; set; }

        DataType DataType { get; }

        void Write(void* fieldAddress, SerializerOutput output);

        void Read(void* fieldAddress, SerializerInput input);
    }

    public unsafe class CustomSerializerProvider : ISerializationProvider
    {
        private TypeDictionary<ICustomSerializer> _instances;

        public void Initialize(USerializer serializer)
        {
            _instances = new TypeDictionary<ICustomSerializer>(512);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var serializers = assembly.GetCustomAttributes<CustomSerializerAttribute>();

                foreach (var attribute in serializers)
                {
                    if (typeof(ICustomSerializer).IsAssignableFrom(attribute.SerializerType) == false)
                    {
                        Debug.LogError($"{attribute.SerializerType} does not inherit ICustomSerializer");
                        continue;
                    }

                    var instance = (ICustomSerializer)Activator.CreateInstance(attribute.SerializerType);
                    instance.Serializer = serializer;
                    _instances.Add(instance.SerializedType, instance);
                }
            }
        }

        public bool TryGetSerializationMethods(Type type, out SerializationMethods serializationMethods)
        {
            if (_instances.TryGetValue(type, out var instance))
            {
                serializationMethods = new SerializationMethods(instance.Write, instance.Read, instance.DataType);
                return true;
            }

            serializationMethods = default;
            return false;
        }

        public bool TryGetInstance<T>(Type serializedType, out T result) where T : class
        {
            if (_instances.TryGetValue(serializedType, out var instance))
            {
                result = instance as T;
                return result != null;
            }

            result = default;
            return false;
        }
    }
}