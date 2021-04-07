using System;
using System.Reflection;
using UnityEngine;

namespace USerialization
{
    public interface ICustomSerializer
    {
        Type SerializedType { get; }

        void Initialize(USerializer serializer);

        SerializationMethods GetMethods();
    }

    public class CustomSerializerProvider : ISerializationProvider
    {
        private TypeDictionary<ICustomSerializer> _instances;

        public void Initialize(USerializer serializer)
        {
            _instances = new TypeDictionary<ICustomSerializer>(512);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var customSerializerType = typeof(ICustomSerializer);

            foreach (var assembly in assemblies)
            {
                var serializers = assembly.GetCustomAttributes<CustomSerializerAttribute>();

                foreach (var attribute in serializers)
                {
                    if (customSerializerType.IsAssignableFrom(attribute.SerializerType) == false)
                    {
                        Debug.LogError($"{attribute.SerializerType} does not inherit ICustomSerializer");
                        continue;
                    }

                    var instance = (ICustomSerializer)Activator.CreateInstance(attribute.SerializerType);
                    _instances.Add(instance.SerializedType, instance);
                }
            }
        }

        public void Start(USerializer serializer)
        {
            foreach (var instance in _instances.Values())
            {
                instance.Initialize(serializer);
            }
        }

        public bool TryGetSerializationMethods(Type type, out SerializationMethods serializationMethods)
        {
            if (_instances.TryGetValue(type, out var instance))
            {
                serializationMethods = instance.GetMethods();// new SerializationMethods(instance.Write, instance.Read, instance.DataType);
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