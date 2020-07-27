using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Object = System.Object;

namespace USerialization
{
    public unsafe delegate void WriteDelegate(void* fieldAddress, SerializerOutput output);

    public unsafe delegate void ReadDelegate(void* fieldAddress, SerializerInput input);

    public readonly struct SerializationMethods
    {
        public readonly WriteDelegate Serialize;
        public readonly ReadDelegate Deserialize;

        public SerializationMethods(WriteDelegate serialize, ReadDelegate deserialize)
        {
            Serialize = serialize;
            Deserialize = deserialize;
        }
    }

    public class USerializer
    {

        private TypeDictionary<TypeData> _datas = new TypeDictionary<TypeData>(1024);

        private static readonly ISerializationProvider[] Providers = {
            new CustomSerializerProvider(),
            new EnumSerializer(),

            //new ComponentProvider(),
            //new GameObjectSerializer(),

            new ClassSerializer(),
            new StructSerializer(),
            new ArraySerializer(),
            new ListSerializer(),
        };

        private TypeDictionary<SerializationMethods> _methods = new TypeDictionary<SerializationMethods>(1024);

        public USerializer()
        {
            foreach (var serializationProvider in Providers)
            {
                serializationProvider.Initialize(this);
            }
        }

        public T GetProvider<T>() where T : ISerializationProvider
        {
            foreach (var serializationProvider in Providers)
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

            foreach (var provider in Providers)
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

        public List<FieldData> GetFields(Type type)//todo use a pooled list for field data
        {
            var allFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);//todo get inherited fields too

            var length = allFields.Length;
            var fields = new List<FieldData>(length);

            for (int i = 0; i < length; i++)
            {
                var fieldInfo = allFields[i];
                if (fieldInfo.IsPrivate)
                {
                    if (Attribute.IsDefined(fieldInfo, typeof(SerializeField)) == false)//todo cache typeof
                        continue;
                }
                else
                {
                    if (Attribute.IsDefined(fieldInfo, typeof(NonSerializedAttribute)))//todo cache typeof
                        continue;
                }

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

        public static bool ShouldProvide(Type type)
        {
            if (type.IsAbstract)
                return false;

            if (type.IsGenericType)// Type<int>
                return false;

            if (type.IsGenericTypeDefinition)// Type<>
                return false;

            if (type.IsClass)
            {
                if (typeof(Object).IsAssignableFrom(type))
                    return true;

                if (type.GetCustomAttribute<SerializableAttribute>() != null)
                    return true;
            }

            if (type.IsValueType)
            {
                return true;
            }

            return false;
        }

        public bool GetTypeData(Type type, out TypeData typeData)
        {
            if (_datas.TryGetValue(type, out typeData))
            {
                return typeData.Fields != null;
            }

            if (ShouldProvide(type))
            {
                typeData = new TypeData();
                typeData.Type = type;
                typeData.Fields = Array.Empty<FieldData>();
                _datas.Add(type, typeData);//to prevent recursion when GetFields

                typeData.Fields = GetFields(type).ToArray();

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
            if (type != result.GetType())
                throw new Exception("Invalid type passed!");

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