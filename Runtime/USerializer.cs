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
        private readonly ISerializationPolicy _serializationPolicy;

        private readonly TypeDictionary<TypeData> _datas = new TypeDictionary<TypeData>(1024);

        private readonly ISerializationProvider[] _providers;

        private readonly TypeDictionary<SerializationMethods> _methods = new TypeDictionary<SerializationMethods>(1024);

        public ISerializationProvider[] Providers => _providers;

        public USerializer(ISerializationPolicy serializationPolicy, ISerializationProvider[] providers)
        {
            _serializationPolicy = serializationPolicy;
            _providers = providers;

            foreach (var serializationProvider in _providers)
                serializationProvider.Initialize(this);

            foreach (var serializationProvider in _providers)
                serializationProvider.Start(this);
        }

        public bool TryGetSerializationMethods(Type type, out SerializationMethods methods)
        {
            if (_methods.TryGetValue(type, out methods))
            {
                return methods.Serialize != null;
            }

            foreach (var provider in _providers)
            {
                if (provider.TryGetSerializationMethods(type, out methods) == false)
                    continue;

                _methods.Add(type, methods);
                return true;
            }

            methods = default;
            _methods.Add(type, methods);
            return false;
        }

        public bool TryGetSerializationMethods<T>(out TypedSerializationMethods<T> methods)
        {
            var type = typeof(T);

            if (_methods.TryGetValue(type, out var result))
            {
                if (result.Serialize != null)
                {
                    methods = new TypedSerializationMethods<T>(result);
                    return true;
                }

                methods = default;
                return false;
            }

            foreach (var provider in _providers)
            {
                if (provider.TryGetSerializationMethods(type, out result) == false)
                    continue;

                _methods.Add(type, result);
                methods = new TypedSerializationMethods<T>(result);
                return true;
            }

            methods = default;
            _methods.Add(type, result);
            return false;
        }

        public bool TryGetNonCachedSerializationMethods(Type type, out SerializationMethods methods, Func<ISerializationProvider, bool> shouldUse = null)
        {
            foreach (var provider in _providers)
            {
                if (shouldUse != null
                && shouldUse(provider) == false)
                    continue;

                if (provider.TryGetSerializationMethods(type, out methods) == false)
                    continue;

                return true;
            }

            methods = default;
            return false;
        }


        public FieldData[] GetFields(Type type)
        {
            var allFields = TypeUtils.GetAllFields(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var length = allFields.Count;

            var fields = new FieldData[length];
            var index = 0;

            foreach (var fieldInfo in allFields)
            {
                if (_serializationPolicy.ShouldSerialize(fieldInfo) == false)
                    continue;

                if (TryGetSerializationMethods(fieldInfo.FieldType, out var serializationMethods) == false)
                    continue;

                var fieldOffset = UnsafeUtility.GetFieldOffset(fieldInfo);
                if (fieldOffset > short.MaxValue)
                    throw new Exception("Field offset way to big!");

                fields[index] = new FieldData(fieldInfo, serializationMethods, (ushort)fieldOffset);
                index++;
            }

            if (index != fields.Length)
                Array.Resize(ref fields, index);

            return fields;
        }


        public bool GetTypeData(Type type, out TypeData typeData)
        {
            if (_datas.TryGetValue(type, out typeData))
                return typeData != null;

            if (_serializationPolicy.ShouldSerialize(type) == false)
            {
                _datas.Add(type, default);
                return false;
            }

            typeData = new TypeData(type);
            _datas.Add(type, typeData); //to prevent recursion when GetFields

            var fieldDatas = GetFields(type);

            TypeData.OrderFields(fieldDatas);
            typeData.Fields = fieldDatas;

            _datas.Add(type, typeData);

            return true;
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

            return TryGetSerializationMethods(type, out _);
        }

        public unsafe bool Serialize([NotNull] SerializerOutput output, object o)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            Profiler.BeginSample("USerializer.Serialize");

            var type = o.GetType();

            if (TryGetSerializationMethods(type, out var serializationMethods))
            {
                if (type.IsValueType)
                {
                    var handle = GCHandle.Alloc(o, GCHandleType.Pinned);//i would have used unsafe utility but that will not skip internal boxed offsets
                    var intPtr = handle.AddrOfPinnedObject();
                    serializationMethods.Serialize(intPtr.ToPointer(), output);
                    handle.Free();
                }
                else
                {
                    var fieldAddress = Unsafe.AsPointer(ref o);
                    serializationMethods.Serialize(fieldAddress, output);
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

            if (TryGetSerializationMethods(type, out var serializationMethods))
            {
                var fieldAddress = Unsafe.AsPointer(ref value);
                serializationMethods.Serialize(fieldAddress, output);
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

            if (TryGetSerializationMethods(typeof(T), out var serializationMethods))
            {
                result = default;
                serializationMethods.Deserialize(Unsafe.AsPointer(ref result), input);
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

            if (TryGetSerializationMethods(type, out var serializationMethods) == false)
            {
                Profiler.EndSample();
                Debug.LogError($"Could not find serialization data for {type}");
                return false;
            }

            serializationMethods.Deserialize(Unsafe.AsPointer(ref result), input);
            Profiler.EndSample();
            return true;
        }

        public unsafe bool TryPopulateObject([NotNull] SerializerInput input, object obj)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var type = obj.GetType();

            Profiler.BeginSample("USerializer.TryPopulateObject");

            if (TryGetSerializationMethods(type, out var serializationMethods) == false)
            {
                Profiler.EndSample();
                Debug.LogError($"Could not find serialization data for {type}");
                return false;
            }

            serializationMethods.Deserialize(Unsafe.AsPointer(ref obj), input);
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