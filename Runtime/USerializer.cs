using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;

namespace USerialization
{
    public unsafe delegate void WriteDelegate(void* fieldAddress, SerializerOutput output);

    public unsafe delegate void ReadDelegate(void* fieldAddress, SerializerInput input);

    public class USerializer
    {
        private readonly ISerializationProvider[] _providers;

        private readonly TypeDictionary<DataSerializer> _methods = new TypeDictionary<DataSerializer>(1024);

        public ISerializationProvider[] Providers => _providers;

        public ISerializationPolicy SerializationPolicy { get; }

        public DataTypesDatabase DataTypesDatabase { get; private set; }

        public USerializer(ISerializationPolicy serializationPolicy, ISerializationProvider[] providers, DataTypesDatabase dataTypesDatabase)
        {
            SerializationPolicy = serializationPolicy;
            _providers = providers;
            DataTypesDatabase = dataTypesDatabase;

            foreach (var serializationProvider in _providers)
                serializationProvider.Initialize(this);

            foreach (var serializationProvider in _providers)
                serializationProvider.Start(this);
        }

        public bool TryGetDataSerializer(Type type, out DataSerializer dataSerializer)
        {
            if (_methods.TryGetValue(type, out dataSerializer))
            {
                return dataSerializer != null;
            }

            foreach (var provider in _providers)
            {
                if (provider.TryGet(type, out dataSerializer) == false)
                    continue;

                _methods.Add(type, dataSerializer);
                return true;
            }

            dataSerializer = default;
            _methods.Add(type, dataSerializer);
            return false;
        }

        public bool TryGetDataSerializer<T>(out TypedDataSerializer<T> methods)
        {
            var type = typeof(T);

            if (_methods.TryGetValue(type, out var result))
            {
                if (result != null)
                {
                    methods = new TypedDataSerializer<T>(result);
                    return true;
                }

                methods = default;
                return false;
            }

            foreach (var provider in _providers)
            {
                if (provider.TryGet(type, out result) == false)
                    continue;

                _methods.Add(type, result);
                methods = new TypedDataSerializer<T>(result);
                return true;
            }

            methods = default;
            _methods.Add(type, result);
            return false;
        }

        public bool TryGetNonCachedSerializationMethods(Type type, out DataSerializer dataSerializer, Func<ISerializationProvider, bool> shouldUse = null)
        {
            foreach (var provider in _providers)
            {
                if (shouldUse != null
                && shouldUse(provider) == false)
                    continue;

                if (provider.TryGet(type, out dataSerializer) == false)
                    continue;

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
        public bool PreCacheType([NotNull] Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return TryGetDataSerializer(type, out _);
        }

        public unsafe bool Serialize([NotNull] SerializerOutput output, object o)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            Profiler.BeginSample("USerializer.Serialize");

            var type = o.GetType();

            if (TryGetDataSerializer(type, out var serializationMethods))
            {
                if (type.IsValueType)
                {
                    var handle = GCHandle.Alloc(o, GCHandleType.Pinned);//i would have used unsafe utility but that will not skip internal boxed offsets
                    var intPtr = handle.AddrOfPinnedObject();
                    serializationMethods.WriteDelegate(intPtr.ToPointer(), output);
                    handle.Free();
                }
                else
                {
                    var fieldAddress = Unsafe.AsPointer(ref o);
                    serializationMethods.WriteDelegate(fieldAddress, output);
                }

                Profiler.EndSample();
                return true;
            }

            Profiler.EndSample();
            return false;
        }

        public unsafe bool SerializeTyped<T>([NotNull] SerializerOutput output, T value) where T : struct
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            Profiler.BeginSample("USerializer.SerializeTyped");
            var type = value.GetType();

            if (TryGetDataSerializer(type, out var serializationMethods))
            {
                var fieldAddress = Unsafe.AsPointer(ref value);
                serializationMethods.WriteDelegate(fieldAddress, output);
                Profiler.EndSample();
                return true;
            }
            Profiler.EndSample();
            return true;
        }


        public unsafe bool TryDeserialize<T>([NotNull] SerializerInput input, out T result)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            Profiler.BeginSample("USerializer.TryDeserialize");

            if (TryGetDataSerializer(typeof(T), out var serializationMethods))
            {
                result = default;
                serializationMethods.ReadDelegate(Unsafe.AsPointer(ref result), input);
                Profiler.EndSample();
                return true;
            }
            Profiler.EndSample();
            result = default;
            return false;
        }

        public bool TryPopulateObject<T>([NotNull] SerializerInput input, ref T result)
        {
            return TryPopulateObject(input, typeof(T), ref result);
        }

        public unsafe bool TryPopulateObject<T>([NotNull] SerializerInput input, Type type, ref T result)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (typeof(T).IsAssignableFrom(type) == false)
            {
                Debug.LogError($"You are try to use a serializer:{type}  on a non compatible type {typeof(T)}");
                return false;
            }

            Profiler.BeginSample("USerializer.TryPopulateObject");

            if (TryGetDataSerializer(type, out var serializationMethods) == false)
            {
                Profiler.EndSample();
                Debug.LogError($"Could not find serialization data for {type}");
                return false;
            }

            serializationMethods.ReadDelegate(Unsafe.AsPointer(ref result), input);
            Profiler.EndSample();
            return true;
        }

        public unsafe bool TryPopulateObject([NotNull] SerializerInput input, object obj)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var type = obj.GetType();

            Profiler.BeginSample("USerializer.TryPopulateObject");

            if (TryGetDataSerializer(type, out var serializationMethods) == false)
            {
                Profiler.EndSample();
                Debug.LogError($"Could not find serialization data for {type}");
                return false;
            }

            serializationMethods.ReadDelegate(Unsafe.AsPointer(ref obj), input);
            Profiler.EndSample();
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