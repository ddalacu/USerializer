using System;
using System.Runtime.CompilerServices;
using System.Threading;


namespace USerialization
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class LocalModuleInitializeAttribute : Attribute
    {
        public Type TargetType { get; }
        public string Method { get; }
        public int Order { get; }

        public LocalModuleInitializeAttribute(Type targetType, string method, int order = 0)
        {
            TargetType = targetType;
            Method = method;
            Order = order;
        }
    }

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

        /// <summary>
        /// Forces caching type data required for serializing a type, use this so you don't get the penality when encountering a type first time
        /// </summary>
        /// <param name="type"></param>
        /// <returns>tells if type can be serialized</returns>
        public bool PreCacheType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return TryGetDataSerializer(type, out _);
        }

        public unsafe bool Serialize(SerializerOutput output, object o, object context = null)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var type = o.GetType();

            if (TryGetDataSerializer(type, out var serializationMethods))
            {
                if (type.IsValueType)
                {
                    var pinnable = Unsafe.As<object, PinnableObject>(ref o);
                    fixed (byte* objectAddress = &pinnable.Pinnable)
                    {
                        serializationMethods.Write(objectAddress, output, context);
                    }
                }
                else
                {
                    var fieldAddress = Unsafe.AsPointer(ref o);
                    serializationMethods.Write(fieldAddress, output, context);
                }

                return true;
            }

            return false;
        }

        public unsafe bool Serialize<T>(SerializerOutput output, ref T value, object context = null) where T : struct
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var type = typeof(T);

            if (TryGetDataSerializer(type, out var serializationMethods))
            {
                var fieldAddress = Unsafe.AsPointer(ref value);
                serializationMethods.Write(fieldAddress, output, context);
                return true;
            }

            return true;
        }

        public unsafe bool TryDeserialize<T>(SerializerInput input, ref T result, object context = null)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            DataSerializer serializationMethods;

            var type = typeof(T);

            if (result == null)
            {
                if (TryGetDataSerializer(type, out serializationMethods) == false)
                    return false;
            }
            else
            {
                if (type.IsValueType)
                {
                    if (TryGetDataSerializer(type, out serializationMethods) == false)
                        return false;
                }
                else
                {
                    if (TryGetDataSerializer(result.GetType(), out serializationMethods) == false)
                        return false;
                }
            }

            serializationMethods.Read(Unsafe.AsPointer(ref result), input, context);
            return true;
        }

        public unsafe bool TryDeserialize<T>(SerializerInput input, Type type, ref T result, object context = null)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (typeof(T).IsAssignableFrom(type) == false)
                throw new Exception($"You are try to use a serializer:{type}  on a non compatible type {typeof(T)}");

            if (TryGetDataSerializer(type, out var serializationMethods) == false)
                return false;

            serializationMethods.Read(Unsafe.AsPointer(ref result), input, context);
            return true;
        }

        public bool TryGetProvider<T>(out T result) where T : ISerializationProvider
        {
            foreach (var serializationProvider in _providers)
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