using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace USerialization
{
    public unsafe class ClassSerializationProvider : ISerializationProvider
    {
        private USerializer _serializer;

        private TypeDataCache _typeDataCache;

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
            _typeDataCache = new TypeDataCache(512);
        }

        public void Start(USerializer serializer)
        {

        }

        public bool TryGet(Type type, out DataSerializer serializationMethods)
        {
            if (type.IsArray)
            {
                serializationMethods = default;
                return false;
            }

            if (type.IsValueType)
            {
                serializationMethods = default;
                return false;
            }

            if (type.IsPrimitive)
            {
                serializationMethods = default;
                return false;
            }

            if (_typeDataCache.GetTypeData(type, _serializer, out var typeData) == false)
            {
                serializationMethods = default;
                return false;
            }

            serializationMethods = GetSerializationMethods(type, typeData);
            return true;
        }

        public DataSerializer GetSerializationMethods(Type type, TypeData typeData)
        {
            if (typeof(ISerializationCallbackReceiver).IsAssignableFrom(type))
                return new ClassWithEventsDataSerializer(type, typeData);

            return new ClassDataSerializer(type, typeData);
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public sealed class ClassDataSerializer : DataSerializer
        {
            private readonly Type _type;
            private readonly TypeData _typeData;
            private readonly bool _haveCtor;

            public ClassDataSerializer(Type type, TypeData typeData) : base(DataType.Object)
            {
                if (type.IsValueType)
                    throw new ArgumentException(nameof(type));

                _type = type;
                _typeData = typeData;

                var ctor = _type.GetConstructor(Type.EmptyTypes);
                _haveCtor = ctor != null;
            }

            public override void WriteDelegate(void* fieldAddress, SerializerOutput output)
            {
                var obj = Unsafe.Read<object>(fieldAddress);

                if (obj == null)
                {
                    output.WriteNull();
                    return;
                }

                byte* objectAddress;
                UnsafeUtility.CopyObjectAddressToPtr(obj, &objectAddress);

                var track = output.BeginSizeTrack();
                _typeData.Fields.WriteFields(objectAddress, output);
                output.WriteSizeTrack(track);
            }

            public override void ReadDelegate(void* fieldAddress, SerializerInput input)
            {
                ref var instance = ref Unsafe.AsRef<object>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    if (instance == null)
                    {
                        if (_haveCtor)
                            instance = Activator.CreateInstance(_type);
                        else
                            instance = FormatterServices.GetUninitializedObject(_type);
                    }

                    byte* objectAddress;
                    UnsafeUtility.CopyObjectAddressToPtr(instance, &objectAddress);

                    _typeData.Fields.ReadFields(objectAddress, input);

                    input.EndObject(end);
                }
                else
                {
                    instance = null;
                }
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public sealed class ClassWithEventsDataSerializer : DataSerializer
        {
            private readonly Type _type;
            private readonly TypeData _typeData;
            private readonly bool _haveCtor;

            public ClassWithEventsDataSerializer(Type type, TypeData typeData) : base(DataType.Object)
            {
                if (type.IsValueType)
                    throw new ArgumentException(nameof(type));

                _type = type;
                _typeData = typeData;

                var ctor = _type.GetConstructor(Type.EmptyTypes);
                _haveCtor = ctor != null;
            }

            public override void WriteDelegate(void* fieldAddress, SerializerOutput output)
            {
                var obj = Unsafe.Read<object>(fieldAddress);

                if (obj == null)
                {
                    output.WriteNull();
                    return;
                }

                Unsafe.As<ISerializationCallbackReceiver>(obj).OnBeforeSerialize();

                byte* objectAddress;
                UnsafeUtility.CopyObjectAddressToPtr(obj, &objectAddress);

                var track = output.BeginSizeTrack();
                _typeData.Fields.WriteFields(objectAddress, output);
                output.WriteSizeTrack(track);
            }

            public override void ReadDelegate(void* fieldAddress, SerializerInput input)
            {
                ref var instance = ref Unsafe.AsRef<object>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    if (instance == null)
                    {
                        if (_haveCtor)
                            instance = Activator.CreateInstance(_type);
                        else
                            instance = FormatterServices.GetUninitializedObject(_type);
                    }

                    byte* objectAddress;
                    UnsafeUtility.CopyObjectAddressToPtr(instance, &objectAddress);

                    _typeData.Fields.ReadFields(objectAddress, input);

                    Unsafe.As<ISerializationCallbackReceiver>(instance).OnAfterDeserialize();

                    input.EndObject(end);
                }
                else
                {
                    instance = null;
                }
            }
        }

    }
}