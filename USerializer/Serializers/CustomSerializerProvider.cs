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

        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                        _log.Error($"{attribute.SerializerType} does not inherit {nameof(CustomDataSerializer)}");
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
                    throw new NotImplementedException();
                    //Debug.LogError($"{value} should assign data type inside TryInitialize!");
                }
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