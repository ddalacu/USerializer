using System;
using System.Reflection;
using System.Runtime.CompilerServices;


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

    public delegate IntPtr GetFunctionPointerDelegate(MethodInfo methodInfo);

    
    public class USerializer
    {
        private readonly ISerializationProvider[] _providers;

        private readonly TypeDictionary<DataSerializer> _methods = new TypeDictionary<DataSerializer>(1024);

        public ISerializationProvider[] Providers => _providers;

        public ISerializationPolicy SerializationPolicy { get; }

        public DataTypesDatabase DataTypesDatabase { get; private set; }

        public ILogger Logger { get; set; }

        public GetFunctionPointerDelegate GetFunctionPointer { get; set; }

        public USerializer(ISerializationPolicy serializationPolicy, ISerializationProvider[] providers, DataTypesDatabase dataTypesDatabase, ILogger logger)
        {
            SerializationPolicy = serializationPolicy;
            _providers = providers;
            DataTypesDatabase = dataTypesDatabase;
            Logger = logger;
            GetFunctionPointer = DefaultGetFunctionPointer;
        }

        private static IntPtr DefaultGetFunctionPointer(MethodInfo methodinfo)
        {
            return methodinfo.MethodHandle.GetFunctionPointer();
        }

        public bool TryGetDataSerializer(Type type, out DataSerializer dataSerializer, bool initializeDataSerializer = true)
        {
            if (type == null) 
                throw new ArgumentNullException(nameof(type));
            
            if (_methods.TryGetValue(type, out dataSerializer))
                return dataSerializer != null;

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

        public bool TryGetNonCachedSerializationMethods(Type type, out DataSerializer dataSerializer, Func<ISerializationProvider, bool> shouldUse = null)
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

        public unsafe bool Serialize(SerializerOutput output, object o)
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
                        serializationMethods.WriteMethod.Invoke(objectAddress, output);
                    }
                }
                else
                {
                    var fieldAddress = Unsafe.AsPointer(ref o);
                    serializationMethods.WriteMethod.Invoke(fieldAddress, output);
                }

                return true;
            }

            return false;
        }

        public unsafe bool SerializeTyped<T>(SerializerOutput output, T value) where T : struct
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var type = value.GetType();

            if (TryGetDataSerializer(type, out var serializationMethods))
            {
                var fieldAddress = Unsafe.AsPointer(ref value);
                serializationMethods.WriteMethod.Invoke(fieldAddress, output);
                return true;
            }

            return true;
        }


        public unsafe bool TryDeserialize<T>(SerializerInput input, ref T result)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (TryGetDataSerializer(typeof(T), out var serializationMethods))
            {
                serializationMethods.ReadMethod.Invoke(Unsafe.AsPointer(ref result), input);
                //serializationMethods.Read(Unsafe.AsPointer(ref result), input);
                return true;
            }
            else
                result = default;
            return false;
        }

        public bool TryPopulateObject<T>(SerializerInput input, ref T result)
        {
            return TryPopulateObject(input, typeof(T), ref result);
        }

        public unsafe bool TryPopulateObject<T>(SerializerInput input, Type type, ref T result)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (typeof(T).IsAssignableFrom(type) == false)
            {
                throw new Exception($"You are try to use a serializer:{type}  on a non compatible type {typeof(T)}");
                //return false;
            }

            if (TryGetDataSerializer(type, out var serializationMethods) == false)
            {
                //Debug.LogError($"Could not find serialization data for {type}");
                return false;
            }

            serializationMethods.ReadMethod.Invoke(Unsafe.AsPointer(ref result), input);
            return true;
        }

        public unsafe bool TryPopulateObject(SerializerInput input, object obj)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var type = obj.GetType();

            if (TryGetDataSerializer(type, out var serializationMethods) == false)
            {
                //Debug.LogError($"Could not find serialization data for {type}");
                return false;
            }

            serializationMethods.ReadMethod.Invoke(Unsafe.AsPointer(ref obj), input);
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