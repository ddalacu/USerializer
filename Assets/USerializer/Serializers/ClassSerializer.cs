﻿using System;
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

            serializationMethods = new SerializationMethods(writer, reader);

            return true;
        }

        private static WriteDelegate GetWriter(TypeData typeData)
        {
            return delegate (void* fieldAddress, SerializerOutput output)
            {
                var obj = Unsafe.Read<object>(fieldAddress);

                if (obj != null)
                {
                    output.OpenObject();

                    var fieldDatas = typeData.Fields;
                    var fieldsLength = fieldDatas.Length;


                    byte* objectAddress;
                    UnsafeUtility.CopyObjectAddressToPtr(obj, &objectAddress);

                    for (var index = 0; index < fieldsLength; index++)
                    {
                        var fieldData = fieldDatas[index];
                        output.OpenField(fieldData.FieldInfo.Name);
                        fieldData.SerializationMethods.Serialize(objectAddress + fieldData.Offset, output);
                        output.CloseField();
                    }


                    output.CloseObject();
                }
                else
                {
                    output.Null();
                }
            };
        }

        private static ReadDelegate GetReader(Type fieldType, TypeData typeData)
        {
            return delegate (void* fieldAddress, SerializerInput input)
            {
                ref var instance = ref Unsafe.AsRef<object>(fieldAddress);


                if (input.BeginReadObject(out var enumerator))
                {
                    if (instance == null)
                        instance = Activator.CreateInstance(fieldType);

                    byte* objectAddress;
                    UnsafeUtility.CopyObjectAddressToPtr(instance, &objectAddress);

                    var fieldDatas = typeData.Fields;
                    var fieldsLength = fieldDatas.Length;

                    var fieldIndex = 0;
                    while (enumerator.Next(ref fieldIndex, out var field))
                    {
                        for (var index = 0; index < fieldsLength; index++)
                        {
                            var fieldData = fieldDatas[index];

                            if (field == fieldData.FieldInfo.Name)
                            {
                                fieldData.SerializationMethods.Deserialize(objectAddress + fieldData.Offset, input);
                                break;
                            }
                        }
                    }

                    input.CloseObject();
                }
                else
                {
                    instance = null;
                }
            };
        }
    }
}