using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Object = System.Object;

namespace USerialization
{
    public unsafe class ArraySerializer : ISerializationProvider
    {
        private USerializer _serializer;

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
        }

        public bool TryGetSerializationMethods(Type type, out SerializationMethods serializationMethods)
        {
            if (type.IsArray == false)
            {
                serializationMethods = default;
                return false;
            }

            if (type.GetArrayRank() > 1)
            {
                serializationMethods = default;
                return false;
            }

            var elementType = type.GetElementType();

            if (_serializer.TryGetSerializationMethods(elementType, out var elementSerializationMethods) == false)
            {
                serializationMethods = default;
                return false;
            }

            if (elementType.IsValueType)
            {
                var writer = GetValueTypeWriter(elementType, elementSerializationMethods.Serialize);
                var reader = GetValueTypeReader(elementType, elementSerializationMethods.Deserialize);

                serializationMethods = new SerializationMethods(writer, reader);
            }
            else
            {
                var writer = GetReferenceTypeWriter(elementSerializationMethods.Serialize);
                var reader = GetReferenceTypeReader(elementType, elementSerializationMethods.Deserialize);

                serializationMethods = new SerializationMethods(writer, reader);
            }

            return true;
        }

        private static WriteDelegate GetValueTypeWriter(Type elementType, WriteDelegate serializeElement)
        {
            var size = UnsafeUtility.SizeOf(elementType);

            return delegate (void* fieldAddress, SerializerOutput output)
             {
                 var array = Unsafe.Read<Array>(fieldAddress);

                 if (array != null)
                 {
                     var count = array.Length;
                     output.OpenArray();

                     var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                     for (var index = 0; index < count; index++)
                     {
                         serializeElement(address, output);
                         address += size;

                         if (index < count - 1)
                             output.WriteArraySeparator();
                     }

                     UnsafeUtility.ReleaseGCObject(handle);
                     output.CloseArray();
                 }
                 else
                 {
                     output.Null();
                 }
             };
        }

        private ReadDelegate GetValueTypeReader(Type elementType, ReadDelegate elementReader)
        {
            var size = UnsafeUtility.SizeOf(elementType);

            return delegate (void* fieldAddress, SerializerInput input)
            {
                ref var array = ref Unsafe.AsRef<Array>(fieldAddress);

                if (input.BeginReadArray(out var count, out var elementEnumerator))
                {
                    array = Array.CreateInstance(elementType, count);

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
                    array = null;
                }
            };
        }

        private static WriteDelegate GetReferenceTypeWriter(WriteDelegate serializeElement)
        {
            return delegate (void* fieldAddress, SerializerOutput output)
            {
                var array = Unsafe.Read<object[]>(fieldAddress);

                if (array != null)
                {
                    var count = array.Length;
                    output.OpenArray();

                    var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                    for (var index = 0; index < count; index++)
                    {
                        var o = array[index];
                        if (o != null)
                        {
                            serializeElement(address, output);
                        }
                        else
                        {
                            output.Null();
                        }

                        address += sizeof(void*);

                        if (index < count - 1)
                            output.WriteArraySeparator();
                    }

                    UnsafeUtility.ReleaseGCObject(handle);

                    output.CloseArray();
                }
                else
                {
                    output.Null();
                }
            };
        }

        private ReadDelegate GetReferenceTypeReader(Type elementType, ReadDelegate elementReader)
        {
            return delegate (void* fieldAddress, SerializerInput input)
            {
                ref var array = ref Unsafe.AsRef<object[]>(fieldAddress);

                if (input.BeginReadArray(out var count, out var elementEnumerator))
                {
                    array = (object[])Array.CreateInstance(elementType, count);

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
                    array = null;
                }
            };
        }

    }
}