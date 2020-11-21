using System;
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

            serializationMethods = new SerializationMethods(writer, reader);
            return true;
        }

        private static WriteDelegate GetWriter(TypeData typeData)
        {
            return delegate (void* fieldAddress, SerializerOutput output)
            {
                Shared.WriteObject(output, typeData.Fields, (byte*)fieldAddress);
            };
        }


        private static ReadDelegate GetReader(TypeData typeData)
        {
            return delegate (void* address, SerializerInput input)
            {
                if (input.BeginReadObject(out var iterator))
                {
                    var fieldDatas = typeData.Fields;
                    var fieldsLength = fieldDatas.Length;

                    var objectAddress = (byte*)address;

                    var fieldIndex = 0;
                    while (iterator.Next(ref fieldIndex, out var field))
                    {
                        for (var index = 0; index < fieldsLength; index++)
                        {
                            var fieldData = fieldDatas[index];

                            if (field == fieldData.FieldInfo.Name)
                            {
                                var fieldDataOffset = objectAddress + fieldData.Offset;
                                fieldData.SerializationMethods.Deserialize(fieldDataOffset, input);
                                break;
                            }
                        }

                        //input.CloseField();
                    }

                    input.CloseObject();
                }
                else
                {
                    Debug.LogError("Changed type?");
                }
            };
        }
    }
}