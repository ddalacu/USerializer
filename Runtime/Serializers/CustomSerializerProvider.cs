using System;
using System.Reflection;
using UnityEngine;

namespace USerialization
{
    public abstract class CustomDataSerializer : DataSerializer
    {
        public abstract Type SerializedType { get; }

        public virtual void Initialize(USerializer serializer)
        {

        }

        protected CustomDataSerializer(DataType dataType) : base(dataType)
        {

        }
    }

    public class CustomSerializerProvider : ISerializationProvider
    {
        private TypeDictionary<CustomDataSerializer> _instances;

        public void Initialize(USerializer serializer)
        {
            _instances = new TypeDictionary<CustomDataSerializer>(512);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var customSerializerType = typeof(CustomDataSerializer);

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

                    var instance = (CustomDataSerializer)Activator.CreateInstance(attribute.SerializerType);

                    if (serializer.SerializationPolicy.ShouldSerialize(instance.SerializedType) == false)
                        continue;

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

        public bool TryGet(Type type, out DataSerializer dataSerializer)
        {
            if (_instances.TryGetValue(type, out var instance))
            {
                dataSerializer = instance;// new SerializationMethods(instance.Write, instance.Read, instance.DataType);
                return true;
            }

            dataSerializer = default;
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