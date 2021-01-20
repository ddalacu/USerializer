using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace USerialization
{
    public unsafe class ClassSerializer : ISerializationProvider
    {
        private USerializer _serializer;

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
        }

        public void Start(USerializer serializer)
        {

        }

        public bool TryGetSerializationMethods(Type type, out SerializationMethods serializationMethods)
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

            if (_serializer.GetTypeData(type, out var typeData) == false)
            {
                serializationMethods = default;
                return false;
            }

            var writer = GetWriter(typeData);
            var reader = GetReader(type, typeData);

            serializationMethods = new SerializationMethods(writer, reader, DataType.Object);

            return true;
        }

        private class ClassWriter
        {
            private readonly bool _callSerializationEvents;
            private readonly TypeData _typeData;

            public ClassWriter(TypeData typeData)
            {
                _typeData = typeData;
                _callSerializationEvents = typeof(ISerializationCallbackReceiver).IsAssignableFrom(typeData.Type);
            }

            [Il2CppSetOption(Option.NullChecks, false)]
            [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
            public void Writer(void* fieldAddress, SerializerOutput output)
            {
                var obj = Unsafe.Read<object>(fieldAddress);

                if (obj == null)
                {
                    output.WriteNull();
                    return;
                }

                if (_callSerializationEvents)
                    Unsafe.As<ISerializationCallbackReceiver>(obj).OnBeforeSerialize();

                byte* objectAddress;
                UnsafeUtility.CopyObjectAddressToPtr(obj, &objectAddress);

                var fieldDatas = _typeData.Fields;
                var fieldsCount = fieldDatas.Length;

                var track = output.BeginSizeTrack();
                {
                    output.WriteByte((byte)fieldsCount);

                    for (var index = 0; index < fieldsCount; index++)
                    {
                        var fieldData = fieldDatas[index];

                        output.EnsureNext(5);
                        output.WriteIntUnchecked(fieldData.FieldNameHash);
                        output.WriteByteUnchecked((byte)fieldData.SerializationMethods.DataType);

                        fieldData.SerializationMethods.Serialize(objectAddress + fieldData.Offset, output);
                    }
                }
                output.WriteSizeTrack(track);
            }

        }

        private static WriteDelegate GetWriter(TypeData typeData)
        {
            return new ClassWriter(typeData).Writer;
        }

        private class ClassReader
        {
            private readonly bool _callSerializationEvents;
            private readonly Type _fieldType;
            private readonly TypeData _typeData;

            private bool _haveCtor;

            public ClassReader(Type fieldType, TypeData typeData)
            {
                if (fieldType.IsValueType)
                    throw new ArgumentException(nameof(fieldType));

                var ctor = fieldType.GetConstructor(Type.EmptyTypes);
                _haveCtor = ctor != null;

                _fieldType = fieldType;
                _typeData = typeData;
                _callSerializationEvents = typeof(ISerializationCallbackReceiver).IsAssignableFrom(typeData.Type);
            }

            [Il2CppSetOption(Option.NullChecks, false)]
            [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
            public void Reader(void* fieldAddress, SerializerInput input)
            {
                ref var instance = ref Unsafe.AsRef<object>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    var fieldsCount = input.ReadByte();

                    if (instance == null)
                    {
                        if (_haveCtor)
                            instance = Activator.CreateInstance(_fieldType);
                        else
                            instance = FormatterServices.GetUninitializedObject(_fieldType);
                    }

                    byte* objectAddress;
                    UnsafeUtility.CopyObjectAddressToPtr(instance, &objectAddress);

                    var fieldDatas = _typeData.Fields;
                    var fieldsLength = fieldDatas.Length;

                    int searchStart = 0;

                    for (var i = 0; i < fieldsCount; i++)
                    {
                        var field = input.ReadInt();
                        var type = (DataType)input.ReadByte();

                        var deserialized = false;

                        for (var searchIndex = searchStart; searchIndex < fieldsLength; searchIndex++)
                        {
                            var fieldData = fieldDatas[searchIndex];

                            if (field == fieldData.FieldNameHash)
                            {
                                if (type == fieldData.SerializationMethods.DataType)
                                {
                                    fieldData.SerializationMethods.Deserialize(objectAddress + fieldData.Offset, input);
                                    deserialized = true;
                                }

                                searchStart = searchIndex + 1;
                                break;
                            }
                        }

                        if (deserialized == false)
                        {
                            if (TypeData.GetAlternate(fieldDatas, type, field, out var alternate))
                            {
                                alternate.SerializationMethods.Deserialize(objectAddress + alternate.Offset, input);
                                //Debug.Log("Found alternate");
                            }
                            else
                            {
                                //Debug.Log($"Skipping field of type {type}");
                                input.SkipData(type);
                            }
                        }
                    }

                    input.EndObject(end);

                    if (_callSerializationEvents)
                        Unsafe.As<ISerializationCallbackReceiver>(instance).OnAfterDeserialize();
                }
                else
                {
                    instance = null;
                }
            }
        }

        private static ReadDelegate GetReader(Type fieldType, TypeData typeData)
        {
            return new ClassReader(fieldType, typeData).Reader;
        }
    }
}