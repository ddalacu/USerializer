using System;
using System.Collections.Generic;
using System.Reflection;

namespace USerialization
{
    public abstract class CustomDataSerializer : DataSerializer
    {
        protected override void Initialize(USerializer serializer)
        {
            
        }

        public virtual bool TryInitialize(USerializer serializer)
        {
            return true;
        }
    }

    public class CustomSerializerProvider : ISerializationProvider
    {
        private readonly Dictionary<Type, Type> _map;

        public CustomSerializerProvider(ILogger logger)
        {
            _map = new Dictionary<Type, Type>(512);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var customSerializerType = typeof(CustomDataSerializer);

            foreach (var assembly in assemblies)
            {
                var serializers = assembly.GetCustomAttributes<CustomSerializerAttribute>();

                foreach (var attribute in serializers)
                {
                    if (customSerializerType.IsAssignableFrom(attribute.SerializerType) == false)
                    {
                        logger.Error($"{attribute.SerializerType} does not inherit {nameof(CustomDataSerializer)}");
                        continue;
                    }

                    _map.Add(attribute.SerializedType, attribute.SerializerType);
                }
            }
        }

        public bool TryGet(USerializer serializer, Type type, out DataSerializer dataSerializer)
        {
            if (_map.TryGetValue(type, out var serializerType))
            {
                var instance = (CustomDataSerializer)Activator.CreateInstance(serializerType);

                if (instance.TryInitialize(serializer) == false)
                {
                    dataSerializer = default;
                    return false;
                }

                dataSerializer = instance;
                return true;
            }

            dataSerializer = default;
            return false;
        }
    }
}