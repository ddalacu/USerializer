using System;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace USerialization
{
    public unsafe class StructSerializer : ISerializationProvider
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

            if (type.IsValueType == false)
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
            var reader = GetReader(typeData);

            serializationMethods = new SerializationMethods(writer, reader, DataType.Object);
            return true;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        private static WriteDelegate GetWriter(TypeData typeData)
        {
            return delegate (void* fieldAddress, SerializerOutput output)
            {
                byte* address = (byte*)fieldAddress;
                var fieldsCount = typeData.Fields.Length;

                var track = output.BeginSizeTrack();
                {
                    output.WriteByte((byte)fieldsCount);

                    for (var index = 0; index < fieldsCount; index++)
                    {
                        var fieldData = typeData.Fields[index];

                        output.EnsureNext(5);
                        output.WriteIntUnchecked(fieldData.FieldNameHash);
                        output.WriteByteUnchecked((byte)fieldData.SerializationMethods.DataType);

                        fieldData.SerializationMethods.Serialize(address + fieldData.Offset, output);
                    }
                }
                output.WriteSizeTrack(track);
            };
        }


        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        private static ReadDelegate GetReader(TypeData typeData)
        {
            void Reader(void* address, SerializerInput input)
            {
                if (input.BeginReadSize(out var end))
                {
                    var fieldCount = input.ReadByte();

                    var fieldDatas = typeData.Fields;
                    var fieldsLength = fieldDatas.Length;

                    var objectAddress = (byte*)address;

                    int searchStart = 0;
                    for (var i = 0; i < fieldCount; i++)
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
                                    var fieldDataOffset = objectAddress + fieldData.Offset;
                                    fieldData.SerializationMethods.Deserialize(fieldDataOffset, input);
                                    deserialized = true;
                                }

                                searchStart = searchIndex + 1;
                                break;
                            }
                        }

                        if (deserialized == false)
                        {
                            if (typeData.GetAlternate(type, field, out var alternate))
                            {
                                alternate.SerializationMethods.Deserialize(objectAddress + alternate.Offset, input);
                            }
                            else
                            {
                                Debug.Log($"Skipping field of type {type}");
                                input.SkipData(type);
                            }
                        }
                    }

                    input.EndObject(end);
                }
                else
                {
                    Debug.LogError("Changed from nullable?");
                }
            }

            return Reader;
        }
    }
}