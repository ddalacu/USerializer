using System;
using System.Collections.Generic;
using System.Reflection;

namespace USerialization
{
    public abstract class CustomDataSerializer : DataSerializer
    {
        public abstract Type SerializedType { get; }

        public virtual bool TryInitialize(USerializer serializer)
        {
            return true;
        }
    }

    public class CustomSerializerProvider : ISerializationProvider
    {
        private Dictionary<Type, CustomDataSerializer> _instances;

        public void Initialize(USerializer serializer)
        {
            _instances = new Dictionary<Type, CustomDataSerializer>(512);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var customSerializerType = typeof(CustomDataSerializer);

            foreach (var assembly in assemblies)
            {
                var serializers = assembly.GetCustomAttributes<CustomSerializerAttribute>();

                foreach (var attribute in serializers)
                {
                    if (customSerializerType.IsAssignableFrom(attribute.SerializerType) == false)
                    {
                        serializer.Logger.Error($"{attribute.SerializerType} does not inherit {nameof(CustomDataSerializer)}");
                        continue;
                    }

                    var instance = (CustomDataSerializer)Activator.CreateInstance(attribute.SerializerType);

                    //if (serializer.SerializationPolicy.ShouldSerialize(instance.SerializedType) == false)
                    //{
                    //    Console.WriteLine(instance.SerializedType);
                    //    continue;
                    //}

                    _instances.Add(instance.SerializedType, instance);
                }
            }
        }

        public void Start(USerializer serializer)
        {
            var toRemove = new List<Type>();

            foreach (var entry in _instances)
            {
                if (entry.Value.TryInitialize(serializer) == false)
                    toRemove.Add(entry.Key);
            }

            foreach (var type in toRemove)
                _instances.Remove(type);

            foreach (var value in _instances.Values)
            {
                if (value.GetDataType() == DataType.None)
                {
                    serializer.Logger.Error($"{value} should assign data type inside TryInitialize!");
                }
            }
        }

        public bool TryGet(Type type, out DataSerializer dataSerializer)
        {
            if (_instances.TryGetValue(type, out var instance))
            {
                dataSerializer = instance;
                return true;
            }

            dataSerializer = default;
            return false;
        }
    }
}