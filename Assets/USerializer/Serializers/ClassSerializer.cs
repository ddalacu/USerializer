using System;
using System.Runtime.CompilerServices;
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

                var fieldsCount = _typeData.Fields.Length;

                var track = output.BeginSizeTrack();
                {
                    output.WriteByte((byte) fieldsCount);

                    for (var index = 0; index < fieldsCount; index++)
                    {
                        var fieldData = _typeData.Fields[index];

                        output.EnsureNext(5);
                        output.WriteIntUnchecked(fieldData.FieldNameHash);
                        output.WriteByteUnchecked((byte) fieldData.SerializationMethods.DataType);

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

            public ClassReader(Type fieldType, TypeData typeData)
            {
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

                    if (instance == null) instance = Activator.CreateInstance(_fieldType);

                    byte* objectAddress;
                    UnsafeUtility.CopyObjectAddressToPtr(instance, &objectAddress);

                    var fieldDatas = _typeData.Fields;
                    var fieldsLength = fieldDatas.Length;

                    for (var i = 0; i < fieldsCount; i++)
                    {
                        var field = input.ReadInt();
                        var type = (DataType)input.ReadByte();

                        var deserialized = false;

                        for (var index = 0; index < fieldsLength; index++)
                        {
                            var fieldData = fieldDatas[index];

                            if (field == fieldData.FieldNameHash && type == fieldData.SerializationMethods.DataType)
                            {
                                fieldData.SerializationMethods.Deserialize(objectAddress + fieldData.Offset, input);
                                deserialized = true;
                                break;
                            }
                        }

                        if (deserialized == false)
                        {
                            input.SkipData(type);
                            //skip field
                            Debug.Log("Skipping field!");
                        }
                    }

                    input.EndObject(end);

                    if (_callSerializationEvents) //todo move in separate action to get rid of if
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