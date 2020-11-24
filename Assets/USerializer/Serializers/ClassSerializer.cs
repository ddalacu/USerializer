using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
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

        private static WriteDelegate GetWriter(TypeData typeData)
        {
            return delegate (void* fieldAddress, SerializerOutput output)
            {
                var obj = Unsafe.Read<object>(fieldAddress);

                var callSerializationEvents = typeof(ISerializationCallbackReceiver).IsAssignableFrom(typeData.Type);

                if (obj != null)
                {
                    if (callSerializationEvents)
                        Unsafe.As<ISerializationCallbackReceiver>(obj).OnBeforeSerialize();

                    byte* objectAddress;
                    UnsafeUtility.CopyObjectAddressToPtr(obj, &objectAddress);

                    Shared.WriteObject(output, typeData.Fields, objectAddress);
                }
                else
                {
                    output.WriteNull();
                }
            };
        }

        private static ReadDelegate GetReader(Type fieldType, TypeData typeData)
        {
            var callSerializationEvents = typeof(ISerializationCallbackReceiver).IsAssignableFrom(typeData.Type);

            return delegate (void* fieldAddress, SerializerInput input)
            {
                ref var instance = ref Unsafe.AsRef<object>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    var fieldsCount = input.ReadByte();

                    if (instance == null)
                        instance = Activator.CreateInstance(fieldType);

                    byte* objectAddress;
                    UnsafeUtility.CopyObjectAddressToPtr(instance, &objectAddress);

                    var fieldDatas = typeData.Fields;
                    var fieldsLength = fieldDatas.Length;

                    for (int i = 0; i < fieldsCount; i++)
                    {
                        var field = input.ReadInt();
                        var type = (DataType)input.ReadByte();

                        var deserialized = false;

                        for (var index = 0; index < fieldsLength; index++)
                        {
                            var fieldData = fieldDatas[index];

                            if (field == fieldData.FieldNameHash && 
                                type == fieldData.SerializationMethods.DataType)
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

                    if (callSerializationEvents)//todo move in separate action to get rid of if
                        Unsafe.As<ISerializationCallbackReceiver>(instance).OnAfterDeserialize();

                    input.EndObject(end);
                }
                else
                {
                    instance = null;
                }
            };
        }
    }
}