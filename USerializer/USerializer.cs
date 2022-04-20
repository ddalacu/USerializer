using System;
using System.Threading;


namespace USerialization
{
    public interface ILogger
    {
        void Error(string error);
    }

    public class USerializer
    {
        private readonly ISerializationProvider[] _providers;

        private readonly TypeDictionary<DataSerializer> _methods =
            new TypeDictionary<DataSerializer>(512);

        private readonly object _lock = new object();

        public ISerializationProvider[] Providers => _providers;

        public ISerializationPolicy SerializationPolicy { get; }

        public DataTypesDatabase DataTypesDatabase { get; private set; }

        public ILogger Logger { get; set; }


        public USerializer(ISerializationPolicy serializationPolicy, ISerializationProvider[] providers,
            DataTypesDatabase dataTypesDatabase, ILogger logger)
        {
            SerializationPolicy = serializationPolicy;
            _providers = providers;
            DataTypesDatabase = dataTypesDatabase;
            Logger = logger;
        }

        public void Clear()
        {
            lock (_lock)
            {
                _methods.Clear();
            }
        }

        public bool TryGetDataSerializer(Type type, out DataSerializer dataSerializer,
            bool initializeDataSerializer = true)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var unlock = false;

            if (Monitor.IsEntered(_lock) == false) //allow only one thread at a time to get data serializer
            {
                Monitor.Enter(_lock);
                unlock = true;
            }

            try
            {
                if (_methods.TryGetValue(type, out dataSerializer))
                {
                    if (initializeDataSerializer && dataSerializer != null)
                        dataSerializer.RootInitialize(this);

                    return dataSerializer != null;
                }

                foreach (var provider in _providers)
                {
                    if (provider.TryGet(this, type, out dataSerializer) == false)
                        continue;

                    _methods.Add(type, dataSerializer);

                    if (initializeDataSerializer)
                        dataSerializer.RootInitialize(this);

                    return true;
                }

                dataSerializer = default;
                _methods.Add(type, dataSerializer);
                return false;
            }
            finally
            {
                if (unlock)
                {
                    Monitor.Exit(_lock);
                }
            }
        }

        public bool TryGetNonCachedSerializationMethods(Type type, out DataSerializer dataSerializer,
            Func<ISerializationProvider, bool> shouldUse = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            foreach (var provider in _providers)
            {
                if (shouldUse != null
                    && shouldUse(provider) == false)
                    continue;

                if (provider.TryGet(this, type, out dataSerializer) == false)
                    continue;

                dataSerializer.RootInitialize(this);

                return true;
            }

            dataSerializer = default;
            return false;
        }
    }

    public static class USErializerExtensions
    {
        public static bool TryGetProvider<T>(this USerializer serializer, out T result) where T : ISerializationProvider
        {
            foreach (var serializationProvider in serializer.Providers)
            {
                if (serializationProvider is T provider)
                {
                    result = provider;
                    return true;
                }
            }

            result = default;
            return false;
        }
    }
}