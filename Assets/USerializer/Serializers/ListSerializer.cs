using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Unity.Collections.LowLevel.Unsafe;

namespace USerialization
{
    public unsafe class ListSerializer : ISerializationProvider
    {
        private USerializer _serializer;

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
        }

        private WriteDelegate GetValueTypeWriter(WriteDelegate elementWriter, Type fieldType, Type elementType)
        {
            var listHelper = ListHelper.Create(fieldType);
            var size = UnsafeUtility.SizeOf(elementType);

            return delegate (void* fieldAddress, SerializerOutput output)
            {
                var list = Unsafe.Read<object>(fieldAddress);

                if (list == null)
                {
                    output.Null();
                    return;
                }

                var array = listHelper.GetArray(list, out var count);

                using (var block = new ValueArrayBlock(output, count, size))
                {
                    var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var hande);

                    for (var index = 0; index < count; index++)
                    {
                        elementWriter(address, output);
                        address += size;

                        if (index < count - 1)
                            block.WriteSeparator();
                    }

                    UnsafeUtility.ReleaseGCObject(hande);
                }
            };
        }

        private ReadDelegate GetValueTypeReader(ReadDelegate elementReader, Type fieldType, Type elementType)
        {
            var listHelper = ListHelper.Create(fieldType);
            var size = UnsafeUtility.SizeOf(elementType);

            return delegate (void* fieldAddress, SerializerInput input)
            {
                ref var list = ref Unsafe.AsRef<object>(fieldAddress);

                if (input.BeginReadArray(out var count, out var elementEnumerator))
                {
                    if (list == null)
                        list = FormatterServices.GetUninitializedObject(fieldType);

                    var array = Array.CreateInstance(elementType, count);

                    listHelper.SetArray(list, array, count);

                    var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                    var index = 0;
                    while (elementEnumerator.Next(ref index))
                    {
                        elementReader(address, input);
                        address += size;
                    }

                    UnsafeUtility.ReleaseGCObject(handle);

                    input.EndArray();
                }
                else
                {
                    list = null;
                }
            };
        }

        private WriteDelegate GetReferenceTypeWriter(WriteDelegate elementWriter, Type fieldType)
        {
            var listHelper = ListHelper.Create(fieldType);

            return delegate (void* fieldAddress, SerializerOutput output)
            {
                var list = Unsafe.Read<IList>(fieldAddress);

                if (list == null)
                {
                    output.Null();
                    return;
                }

                var array = listHelper.GetArray<object>(list, out var count);
                using (var block = new ReferenceArrayBlock(output, count))
                {

                    var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                    for (var index = 0; index < count; index++)
                    {
                        var o = array[index];
                        if (o != null)
                        {
                            elementWriter(address, output);
                        }
                        else
                        {
                            output.Null();
                        }

                        address += sizeof(void*);

                        if (index < count - 1)
                            block.WriteSeparator();
                    }

                    UnsafeUtility.ReleaseGCObject(handle);
                }
            };
        }

        private ReadDelegate GetReferenceTypeReader(ReadDelegate elementReader, Type fieldType, Type elementType)
        {
            var listHelper = ListHelper.Create(fieldType);

            return delegate (void* fieldAddress, SerializerInput input)
            {
                ref var list = ref Unsafe.AsRef<object>(fieldAddress);

                if (input.BeginReadArray(out var count, out var elementEnumerator))
                {
                    if (list == null)
                        list = FormatterServices.GetUninitializedObject(fieldType);

                    var array = Array.CreateInstance(elementType, count);

                    listHelper.SetArray(list, array, count);

                    var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                    var index = 0;
                    while (elementEnumerator.Next(ref index))
                    {
                        elementReader(address, input);
                        address += sizeof(void*);
                    }

                    UnsafeUtility.ReleaseGCObject(handle);

                    input.EndArray();
                }
                else
                {
                    list = null;
                }
            };
        }

        public bool TryGetSerializationMethods(Type type, out SerializationMethods serializationMethods)
        {

            if (type.IsConstructedGenericType == false)
            {
                serializationMethods = default;
                return false;
            }

            if (type.GetGenericTypeDefinition() != typeof(List<>))
            {
                serializationMethods = default;
                return false;
            }

            var elementType = type.GetGenericArguments()[0];

            if (_serializer.TryGetSerializationMethods(elementType, out var elementSerializationMethods) == false)
            {
                serializationMethods = default;
                return false;
            }

            if (elementType.IsValueType)
            {
                serializationMethods = new SerializationMethods(
                    GetValueTypeWriter(elementSerializationMethods.Serialize, type, elementType),
                    GetValueTypeReader(elementSerializationMethods.Deserialize, type, elementType));
            }
            else
            {
                serializationMethods = new SerializationMethods(
                    GetReferenceTypeWriter(elementSerializationMethods.Serialize, type),
                    GetReferenceTypeReader(elementSerializationMethods.Deserialize, type, elementType));
            }

            return true;
        }
    }
}