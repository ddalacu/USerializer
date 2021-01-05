using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace USerialization
{
    public unsafe delegate void WriteDelegate(void* fieldAddress, SerializerOutput output);

    public unsafe delegate void ReadDelegate(void* fieldAddress, SerializerInput input);

    public readonly struct SerializationMethods
    {
        public readonly WriteDelegate Serialize;
        public readonly ReadDelegate Deserialize;

        public readonly DataType DataType;

        public SerializationMethods(WriteDelegate serialize, ReadDelegate deserialize, DataType dataType)
        {
            Serialize = serialize;
            Deserialize = deserialize;
            DataType = dataType;
        }
    }

    public class USerializer
    {
        private readonly ISerializationPolicy _serializationPolicy;

        private readonly TypeDictionary<TypeData> _datas = new TypeDictionary<TypeData>(1024);

        private readonly ISerializationProvider[] _providers;

        private TypeDictionary<SerializationMethods> _methods = new TypeDictionary<SerializationMethods>(1024);

        public USerializer(ISerializationPolicy serializationPolicy, ISerializationProvider[] providers)
        {
            _serializationPolicy = serializationPolicy;
            _providers = providers;

            foreach (var serializationProvider in _providers)
                serializationProvider.Initialize(this);

            foreach (var serializationProvider in _providers)
                serializationProvider.Start(this);
        }

        public T GetProvider<T>() where T : ISerializationProvider
        {
            foreach (var serializationProvider in _providers)
            {
                if (serializationProvider is T instance)
                    return instance;
            }

            return default;
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

        public static ICollection<FieldInfo> GetAllFields(Type type, BindingFlags bindingFlags)
        {
            FieldInfo[] fieldInfos = type.GetFields(bindingFlags);

            // If this class doesn't have a base, don't waste any time
            if (type.BaseType == typeof(object))
            {
                return fieldInfos;
            }

            var currentType = type;
            var fieldComparer = new FieldInfoComparer();
            var fieldInfoList = new HashSet<FieldInfo>(fieldInfos, fieldComparer);

            while (currentType != typeof(object))
            {
                fieldInfos = currentType.GetFields(bindingFlags);
                fieldInfoList.UnionWith(fieldInfos);
                currentType = currentType.BaseType;
            }
            return fieldInfoList;
        }

        public List<FieldData> GetFields(Type type)//todo use a pooled list for field data
        {
            var allFields = GetAllFields(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var length = allFields.Count;

            var fields = new List<FieldData>(length);

            foreach (var fieldInfo in allFields)
            {
                if (_serializationPolicy.ShouldSerialize(fieldInfo) == false)
                    continue;

                if (TryGetSerializationMethods(fieldInfo.FieldType, out var serializationMethods))
                {
                    var fieldOffset = UnsafeUtility.GetFieldOffset(fieldInfo);
                    if (fieldOffset > short.MaxValue)
                        throw new Exception("Wot?");

                    fields.Add(new FieldData(fieldInfo, serializationMethods, (ushort)fieldOffset));
                }
            }


            return fields;
        }


        public bool GetTypeData(Type type, out TypeData typeData)
        {
            if (_datas.TryGetValue(type, out typeData))
                return typeData.Fields != null;

            if (_serializationPolicy.ShouldSerialize(type))
            {
                typeData = new TypeData(type, Array.Empty<FieldData>());
                _datas.Add(type, typeData);//to prevent recursion when GetFields

                var fieldDatas = GetFields(type).ToArray();
                TypeData.ValidateFields(fieldDatas);

                typeData = new TypeData(type, fieldDatas);
                _datas.Add(type, typeData);

                //_datas[type] = typeData;//setting correct array

                return true;
            }

            _datas.Add(type, default);

            return false;
        }

        public bool PreCacheLayout(Type type)
        {
            return TryGetSerializationMethods(type, out _);
        }


        public unsafe bool Serialize(SerializerOutput output, object o)
        {
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

                output.Flush();
            }

            return true;
        }

        public unsafe bool SerializeTyped<T>(SerializerOutput output, T value) where T : struct
        {
            var type = value.GetType();

            if (TryGetSerializationMethods(type, out var serializationMethods))
            {
                var fieldAddress = Unsafe.AsPointer(ref value);
                serializationMethods.Serialize(fieldAddress, output);
                output.Flush();
            }

            return true;
        }


        public unsafe bool TryDeserialize<T>(SerializerInput input, out T result)
        {
            if (TryGetSerializationMethods(typeof(T), out var serializationMethods))
            {
                result = default;
                serializationMethods.Deserialize(Unsafe.AsPointer(ref result), input);
                return true;
            }

            result = default;
            return false;
        }

        public bool TryPopulateObject<T>(SerializerInput input, ref T result)
        {
            return TryPopulateObject(input, typeof(T), ref result);
        }

        public unsafe bool TryPopulateObject<T>(SerializerInput input, Type type, ref T result)
        {
            if (typeof(T).IsAssignableFrom(type) == false)
            {
                Debug.LogError($"You are try to use a serializer:{type}  on a non compatible type {typeof(T)}");
                return false;
            }

            if (TryGetSerializationMethods(type, out var serializationMethods))
            {
                serializationMethods.Deserialize(Unsafe.AsPointer(ref result), input);
                return true;
            }

            return false;
        }

        public unsafe bool DeserializeObject(SerializerInput input, object obj)
        {
            var type = obj.GetType();
            if (TryGetSerializationMethods(type, out var serializationMethods))
            {
                serializationMethods.Deserialize(Unsafe.AsPointer(ref obj), input);
                return true;
            }

            return false;
        }

    }
}